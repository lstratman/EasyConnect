using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace EasyConnect.Protocols.Vnc
{
    internal sealed class AmtKvmProxyProcess : IDisposable
    {
        private readonly Process _process;
        private readonly TaskCompletionSource<int> _ready = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly object _errorLock = new object();
        private readonly StringBuilder _errors = new StringBuilder();
        private volatile bool _stopping;
        private string _logDirectory;

        public event Action<int> StatusChanged;
        public event Action<Exception> Failed;

        private AmtKvmProxyProcess(Process process)
        {
            _process = process;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;
            _process.EnableRaisingEvents = true;
        }

        public static async Task<AmtKvmProxyProcess> StartAsync(string host, ushort port, string username, string domain, SecureString password, CancellationToken cancellationToken)
        {
            string executable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EasyConnect.AmtKvmProxy.exe");
            if (!File.Exists(executable))
                throw new FileNotFoundException("The Intel AMT KVM helper is missing. Rebuild with Intel AMT KVM support enabled.", executable);

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };
            AmtKvmProxyProcess proxy = new AmtKvmProxyProcess(process);
            try
            {
                if (!process.Start())
                    throw new InvalidOperationException("The Intel AMT KVM helper could not be started.");
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await proxy.WriteConfigurationAsync(host, port, username, domain, password, cancellationToken).ConfigureAwait(false);
                Task timeout = Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                Task completed = await Task.WhenAny(proxy._ready.Task, timeout).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (completed != proxy._ready.Task)
                    throw new TimeoutException("The Intel AMT KVM helper did not start listening within 20 seconds.");
                await proxy._ready.Task.ConfigureAwait(false);
                return proxy;
            }
            catch
            {
                await proxy.StopAsync().ConfigureAwait(false);
                throw;
            }
        }

        public int LocalPort
        {
            get { return _ready.Task.Result; }
        }

        private async Task WriteConfigurationAsync(string host, ushort port, string username, string domain, SecureString password, CancellationToken cancellationToken)
        {
            byte[] hostBytes = EncodeField(host);
            byte[] usernameBytes = EncodeField(username);
            byte[] domainBytes = EncodeField(domain);
            byte[] passwordBytes = EncodeSecureField(password);
            try
            {
                Stream stream = _process.StandardInput.BaseStream;
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true))
                {
                    WriteField(writer, hostBytes);
                    WriteField(writer, usernameBytes);
                    WriteField(writer, passwordBytes);
                    WriteField(writer, domainBytes);
                    writer.Write(port);
                    writer.Flush();
                }
                cancellationToken.ThrowIfCancellationRequested();
                await Task.CompletedTask.ConfigureAwait(false);
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }

        private static byte[] EncodeField(string value)
        {
            byte[] bytes = Encoding.Default.GetBytes(value ?? String.Empty);
            if (bytes.Length > 255 || Array.IndexOf(bytes, (byte)0) >= 0)
                throw new ArgumentException("AMT host, username, and domain must be at most 255 bytes and cannot contain null characters.");
            return bytes;
        }

        private static byte[] EncodeSecureField(SecureString value)
        {
            if (value == null || value.Length == 0)
                return new byte[0];
            IntPtr plaintext = Marshal.SecureStringToGlobalAllocAnsi(value);
            try
            {
                int length = 0;
                while (Marshal.ReadByte(plaintext, length) != 0)
                    length++;
                if (length > 255)
                    throw new ArgumentException("The AMT password must be at most 255 bytes.");
                byte[] bytes = new byte[length];
                if (length > 0)
                    Marshal.Copy(plaintext, bytes, 0, length);
                return bytes;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocAnsi(plaintext);
            }
        }

        private static void WriteField(BinaryWriter writer, byte[] value)
        {
            writer.Write((ushort)value.Length);
            writer.Write(value);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            if (e.Data.StartsWith("LOG ", StringComparison.Ordinal))
            {
                try
                {
                    string directory = Encoding.UTF8.GetString(Convert.FromBase64String(e.Data.Substring(4)));
                    string fullPath = Path.GetFullPath(directory);
                    string tempPath = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    if (!fullPath.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(fullPath).StartsWith("EasyConnect-AmtKvm-", StringComparison.Ordinal))
                        throw new InvalidDataException("The Intel AMT helper reported an unsafe log directory.");
                    _logDirectory = fullPath;
                }
                catch (FormatException ex)
                {
                    _ready.TrySetException(new InvalidDataException("The Intel AMT helper reported an invalid log directory.", ex));
                }
                catch (ArgumentException ex)
                {
                    _ready.TrySetException(new InvalidDataException("The Intel AMT helper reported an invalid log directory.", ex));
                }
                return;
            }
            if (e.Data.StartsWith("READY ", StringComparison.Ordinal))
            {
                int port;
                if (Int32.TryParse(e.Data.Substring(6), out port) && port > 0 && port <= UInt16.MaxValue)
                    _ready.TrySetResult(port);
                else
                    _ready.TrySetException(new InvalidDataException("The Intel AMT KVM helper returned an invalid listening port."));
                return;
            }
            if (e.Data.StartsWith("STATUS ", StringComparison.Ordinal))
            {
                int status;
                if (Int32.TryParse(e.Data.Substring(7), out status))
                {
                    Action<int> handler = StatusChanged;
                    if (handler != null)
                        handler(status);
                }
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            lock (_errorLock)
                _errors.AppendLine(e.Data);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            InvalidOperationException failure = new InvalidOperationException(GetFailureMessage());
            if (!_ready.Task.IsCompleted)
            {
                _ready.TrySetException(failure);
                return;
            }
            if (!_stopping)
            {
                Action<Exception> handler = Failed;
                if (handler != null)
                    handler(failure);
            }
        }

        private string GetFailureMessage()
        {
            lock (_errorLock)
                return String.IsNullOrWhiteSpace(_errors.ToString())
                    ? "The Intel AMT KVM helper exited before it was ready."
                    : "The Intel AMT KVM helper failed: " + _errors.ToString().Trim();
        }

        public async Task StopAsync()
        {
            if (_process == null)
                return;
            try
            {
                _stopping = true;
                if (!_process.HasExited)
                {
                    _process.StandardInput.Close();
                    bool exited = await Task.Run(() => _process.WaitForExit(5000)).ConfigureAwait(false);
                    if (!exited)
                    {
                        _process.Kill();
                        await Task.Run(() => _process.WaitForExit()).ConfigureAwait(false);
                    }
                }
                if (_process.HasExited)
                    await Task.Run(() => _process.WaitForExit()).ConfigureAwait(false);
                if (_logDirectory != null && Directory.Exists(_logDirectory))
                {
                    File.Delete(Path.Combine(_logDirectory, "log.txt"));
                    Directory.Delete(_logDirectory);
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                _process.Dispose();
            }
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }
}
