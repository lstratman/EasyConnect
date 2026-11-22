using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace EasyConnect.AmtKvmProxy
{
    internal static class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void ConnectionCallback(IntPtr data);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int StatusCallback(int status, uint clientId);

        [DllImport("KVMLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int KVM_Init(string root, string personal, string password, ConnectionCallback callback);
        [DllImport("KVMLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KVM_Start(ref ushort port);
        [DllImport("KVMLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KVM_Stop();
        [DllImport("KVMLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KVM_Deinit();
        [DllImport("KVMLib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void KVM_RegisterStatusCallback(StatusCallback callback);

        private static byte[] host;
        private static byte[] username;
        private static byte[] password;
        private static byte[] domain;
        private static ushort remotePort;
        private static readonly object OutputLock = new object();
        private static readonly ConnectionCallback Connection = FillConnection;
        private static readonly StatusCallback Status = ReportStatus;

        private static byte[] ReadField(BinaryReader reader)
        {
            int length = reader.ReadUInt16();
            if (length > 255)
                throw new InvalidDataException("AMT connection field exceeds 255 bytes.");
            byte[] value = reader.ReadBytes(length);
            if (value.Length != length || Array.IndexOf(value, (byte)0) >= 0)
                throw new InvalidDataException("Invalid AMT connection field.");
            return value;
        }

        private static void FillConnection(IntPtr data)
        {
            Marshal.Copy(host, 0, data, host.Length);
            Marshal.WriteInt16(data, 256, unchecked((short)remotePort));
            Marshal.Copy(username, 0, IntPtr.Add(data, 258), username.Length);
            Marshal.Copy(password, 0, IntPtr.Add(data, 514), password.Length);
            Marshal.Copy(domain, 0, IntPtr.Add(data, 1544), domain.Length);
        }

        private static int ReportStatus(int status, uint clientId)
        {
            try
            {
                lock (OutputLock)
                {
                    Console.Out.WriteLine("STATUS " + status);
                    Console.Out.Flush();
                }
            }
            catch (IOException)
            {
            }
            return 0;
        }

        private static int Main()
        {
            bool initialized = false;
            bool started = false;
            string logDirectory = null;
            try
            {
                using (BinaryReader input = new BinaryReader(Console.OpenStandardInput()))
                {
                    host = ReadField(input);
                    username = ReadField(input);
                    password = ReadField(input);
                    domain = ReadField(input);
                    remotePort = input.ReadUInt16();
                    if (host.Length == 0 || (remotePort != 16994 && remotePort != 16995))
                        throw new InvalidDataException("Invalid AMT host or port.");

                    DirectorySecurity security = new DirectorySecurity();
                    security.SetAccessRuleProtection(true, false);
                    SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
                    security.SetOwner(user);
                    security.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    logDirectory = Path.Combine(Path.GetTempPath(), "EasyConnect-AmtKvm-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(logDirectory, security);
                    Environment.CurrentDirectory = logDirectory;
                    Console.Out.WriteLine("LOG " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(logDirectory)));
                    Console.Out.Flush();

                    int result = KVM_Init("", "", "", Connection);
                    if (result != 0)
                        throw new InvalidOperationException("Intel KVM initialization failed (status " + result + ").");
                    initialized = true;
                    KVM_RegisterStatusCallback(Status);
                    ushort localPort = 0;
                    result = KVM_Start(ref localPort);
                    if (result != 0 || localPort == 0)
                        throw new InvalidOperationException("Intel KVM proxy start failed (status " + result + ").");
                    started = true;
                    lock (OutputLock)
                    {
                        Console.Out.WriteLine("READY " + localPort);
                        Console.Out.Flush();
                    }
                    while (input.Read() != -1) { }
                }
                return 0;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException || ex is System.Security.SecurityException)
            {
                if (ex is EndOfStreamException)
                    Console.Error.WriteLine("The AMT helper received incomplete connection settings.");
                else if (ex is DllNotFoundException)
                    Console.Error.WriteLine("KVMLib.dll or one of its x86 dependencies is missing.");
                else if (ex is BadImageFormatException)
                    Console.Error.WriteLine("KVMLib.dll is not a compatible 32-bit library.");
                else if (ex is EntryPointNotFoundException)
                    Console.Error.WriteLine("KVMLib.dll is missing a required Intel KVM function.");
                else if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
                    Console.Error.WriteLine("The AMT helper could not create its private log directory.");
                else if (ex is IOException)
                    Console.Error.WriteLine("The AMT helper could not read its startup settings.");
                else
                    Console.Error.WriteLine(ex.Message);
                return 1;
            }
            finally
            {
                if (started)
                    KVM_Stop();
                if (initialized)
                    KVM_Deinit();
                if (password != null)
                    Array.Clear(password, 0, password.Length);
                if (logDirectory != null)
                    Environment.CurrentDirectory = Path.GetTempPath();
            }
        }
    }
}
