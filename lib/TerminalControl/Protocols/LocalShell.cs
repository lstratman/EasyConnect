/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: LocalShell.cs,v 1.6 2011/12/23 07:18:44 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

using Poderosa.Util;
using Poderosa.Forms;
using Poderosa.Plugins;

namespace Poderosa.Protocols {
    internal abstract class LocalShellUtil {

        //接続用ソケットのサポート
        protected static Socket _listener;
        protected static int _localPort;
        //同期
        protected static object _lockObject = new object();

        //接続先のSocketを準備して返す。失敗すればparentを親にしてエラーを表示し、nullを返す。
        internal static ITerminalConnection PrepareSocket(IPoderosaForm parent, ICygwinParameter param) {
            try {
                return new Connector(param).Connect();
            }
            catch (Exception ex) {
                //string key = IsCygwin(param)? "Message.CygwinUtil.FailedToConnect" : "Message.SFUUtil.FailedToConnect";
                string key = "Message.CygwinUtil.FailedToConnect";
                parent.Warning(PEnv.Strings.GetString(key) + ex.Message);
                return null;
            }
        }
        public static Connector AsyncPrepareSocket(IInterruptableConnectorClient client, ICygwinParameter param) {
            Connector c = new Connector(param, client);
            new Thread(new ThreadStart(c.AsyncConnect)).Start();
            return c;
        }


        /// <summary>
        /// Exception from LocalShellUtil
        /// </summary>
        internal class LocalShellUtilException : Exception {
            public LocalShellUtilException(string message)
                : base(message) {
            }
            public LocalShellUtilException(string message, Exception innerException)
                : base(message, innerException) {
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public class Connector : IInterruptable {
            private ICygwinParameter _param;
            private Process _process;
            private IInterruptableConnectorClient _client;
            private Thread _asyncThread;
            private bool _interrupted;

            public Connector(ICygwinParameter param) {
                _param = param;
            }
            public Connector(ICygwinParameter param, IInterruptableConnectorClient client) {
                _param = param;
                _client = client;
            }

            public void AsyncConnect() {
                bool success = false;
                _asyncThread = Thread.CurrentThread;
                try {
                    ITerminalConnection result = Connect();
                    if (!_interrupted) {
                        success = true;
                        Debug.Assert(result != null);
                        ProtocolUtil.FireConnectionSucceeded(_param);
                        _client.SuccessfullyExit(result);
                    }
                }
                catch (Exception ex) {
                    if (!(ex is LocalShellUtilException)) {
                        RuntimeUtil.ReportException(ex);
                    }
                    if (!_interrupted) {
                        _client.ConnectionFailed(ex.Message);
                        ProtocolUtil.FireConnectionFailure(_param, ex.Message);
                    }
                }
                finally {
                    if (!success && _process != null && !_process.HasExited)
                        _process.Kill();
                }
            }
            public void Interrupt() {
                _interrupted = true;
            }

            public ITerminalConnection Connect() {
                lock (_lockObject) {
                    if (_localPort == 0)
                        PrepareListener();
                }

                string cygtermPath = GetCygtermPath();
                if (cygtermPath == null)
                    throw new LocalShellUtilException(PEnv.Strings.GetString("Message.CygwinUtil.CygtermExeNotFound"));

                ITerminalParameter term = (ITerminalParameter)_param.GetAdapter(typeof(ITerminalParameter));

                string args = String.Format("-p {0} -v HOME=\"{1}\" -v TERM=\"{2}\" -s \"{3}\"", _localPort, _param.Home, term.TerminalType, _param.ShellName);
                ProcessStartInfo psi = new ProcessStartInfo(cygtermPath, args);
                PrepareEnv(psi, _param);
                psi.CreateNoWindow = true;
                psi.ErrorDialog = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                try {
                    _process = Process.Start(psi);
                }
                catch (System.ComponentModel.Win32Exception ex) {
                    throw new LocalShellUtilException(PEnv.Strings.GetString("Message.CygwinUtil.FailedToRunCygterm") + ": " + cygtermPath, ex);
                }
                while (true) {
                    List<Socket> chk = new List<Socket>();
                    chk.Add(_listener);
                    Socket.Select(chk, null, null, 100);
                    if (_interrupted)
                        return null;
                    if (chk.Count > 0)
                        break;
                }
                Socket sock = _listener.Accept();
                if (_interrupted)
                    return null;

                TelnetNegotiator neg = new TelnetNegotiator(term.TerminalType, term.InitialWidth, term.InitialHeight);
                TelnetParameter shellparam = new TelnetParameter();
                shellparam.Destination = "localhost";
                shellparam.SetTerminalName(term.TerminalType);
                shellparam.SetTerminalSize(term.InitialWidth, term.InitialHeight);
                TelnetTerminalConnection r = new TelnetTerminalConnection(shellparam, neg, new PlainPoderosaSocket(sock));
                r.Destination = (ITerminalParameter)_param.GetAdapter(typeof(ITerminalParameter)); //TelnetでなくオリジナルのCygwinParamで上書き
                return r;
            }

            private static string GetCygtermPath() {
                const string CYGTERM_DIR = "cygterm";
                const string CYGTERM_EXE = "cygterm.exe";

                // 1st candidate: <assembly's location>/Cygterm/cygterm.exe
                string assyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(Path.Combine(assyDir, CYGTERM_DIR), CYGTERM_EXE);
                if (File.Exists(path))
                    return path;

                IPoderosaApplication app = (IPoderosaApplication)ProtocolsPlugin.Instance.PoderosaWorld.GetAdapter(typeof(IPoderosaApplication));

                // 2nd candidate: <HomeDirectory>/Protocols/Cygterm/cygterm.exe
                // Previous default for the release version.
                path = Path.Combine(Path.Combine(Path.Combine(app.HomeDirectory, "protocols"), CYGTERM_DIR), CYGTERM_EXE);
                if (File.Exists(path))
                    return path;

                // 3rd candidate: <HomeDirectory>/Cygterm/cygterm.exe
                // Previous default for the monolithic version.
                path = Path.Combine(Path.Combine(app.HomeDirectory, CYGTERM_DIR), CYGTERM_EXE);
                if (File.Exists(path))
                    return path;

                // 4th candidate: <HomeDirectory>/cygterm.exe
                path = Path.Combine(app.HomeDirectory, CYGTERM_EXE);
                if (File.Exists(path))
                    return path;

                return null;
            }
        }

        protected static void PrepareListener() {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _localPort = 20345;
            do {
                try {
                    _listener.Bind(new IPEndPoint(IPAddress.Loopback, _localPort));
                    _listener.Listen(1);
                    break;
                }
                catch (Exception) {
                    if (_localPort++ == 20360)
                        throw new Exception("port overflow!!"); //さすがにこれはめったにないはず
                }
            } while (true);

        }

        protected static void PrepareEnv(ProcessStartInfo psi, ICygwinParameter p) {
            string path = psi.EnvironmentVariables["PATH"];
            string cygwinDir = p.CygwinDir;
            if (cygwinDir == null || cygwinDir.Length == 0)
                cygwinDir = CygwinUtil.GuessRootDirectory();
            if (path == null)
                path = String.Empty;
            else if (!path.EndsWith(";"))
                path += ";";
            path += cygwinDir + "\\bin";
            psi.EnvironmentVariables.Remove("PATH");
            psi.EnvironmentVariables.Add("PATH", path);
        }

        public static void Terminate() {
            if (_listener != null)
                _listener.Close();
        }

        private static bool IsCygwin(LocalShellParameter tp) {
            return true;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class SFUUtil {
        public static string DefaultHome {
            get {
                string a = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                //最後の\の後にApplication Dataがあるので
                int t = a.LastIndexOf('\\');
                char drive = a[0];
                return "/dev/fs/" + Char.ToUpper(drive) + a.Substring(2, t - 2).Replace('\\', '/');
            }
        }
        public static string DefaultShell {
            get {
                return "/bin/csh -l";
            }
        }
        public static string GuessRootDirectory() {
            RegistryKey reg = null;
            string keyname = "SOFTWARE\\Microsoft\\Services for UNIX";
            reg = Registry.LocalMachine.OpenSubKey(keyname);
            if (reg == null) {
                //GUtil.Warning(GEnv.Frame, String.Format(PEnv.Strings.GetString("Message.SFUUtil.KeyNotFound"), keyname));
                return "";
            }
            string t = (string)reg.GetValue("InstallPath");
            reg.Close();
            return t;
        }

    }

    /// <summary>
    /// <ja>
    /// Cygwin接続時のパラメータを示すヘルパクラスです。
    /// </ja>
    /// <en>
    /// Helper class that shows parameter when Cygwin connecting.
    /// </en>
    /// </summary>
    /// <exclude/>
    public class CygwinUtil {
        /// <summary>
        /// <ja>
        /// デフォルトのホームディレクトリを返します。
        /// </ja>
        /// <en>
        /// Return the default home directory.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// このプロパティは、「"/home/"+Environment.UserName」の値を返します。
        /// </ja>
        /// <en>
        /// This property returns a value of ["/home/"+Environment.UserName].
        /// </en>
        /// </remarks>
        public static string DefaultHome {
            get {
                return "/home/" + Environment.UserName;
            }
        }
        /// <summary>
        /// <ja>
        /// デフォルトのシェルを返します。
        /// </ja>
        /// <en>
        /// Return the default shell.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// このプロパティは、「/bin/bash -i -l」という文字列を返します。
        /// </ja>
        /// <en>
        /// This property returns the string "/bin/bash -i -l".
        /// </en>
        /// </remarks>
        public static string DefaultShell {
            get {
                return "/bin/bash -i -l";
            }
        }

        /// <summary>
        /// <ja>
        /// デフォルトのCygwinのパスを返します。
        /// </ja>
        /// <en>
        /// Return the default Cygwin path.
        /// </en>
        /// </summary>
        public static string DefaultCygwinDir {
            get {
                return String.Empty;    // not specify
            }
        }

        /// <summary>
        /// <ja>
        /// デフォルトの端末タイプを返します。
        /// </ja>
        /// <en>
        /// Return the default terminal type.
        /// </en>
        /// </summary>
        public static string DefaultTerminalType {
            get {
                return "xterm";
            }
        }

        /// <summary>
        /// <ja>
        /// レジストリを検索し、Cygwinのルートディレクトリを返します。
        /// </ja>
        /// <en>
        /// The registry is retrieved, and the root directory of Cygwin is returned. 
        /// </en>
        /// </summary>
        /// <returns><ja>Cygwinのルートディレクトリと思わしき場所が返されます。</ja><en>A root directory of Cygwin and a satisfactory place are returned. </en></returns>
        public static string GuessRootDirectory() {
            //HKCU -> HKLMの順でサーチ
            string rootDir;
            rootDir = GetCygwinRootDirectory(Registry.CurrentUser, false);
            if (rootDir != null)
                return rootDir;
            rootDir = GetCygwinRootDirectory(Registry.LocalMachine, false);
            if (rootDir != null)
                return rootDir;
            if (IntPtr.Size == 8) {	// we're in 64bit
                rootDir = GetCygwinRootDirectory(Registry.LocalMachine, true);
                if (rootDir != null)
                    return rootDir;
            }

            //TODO 必ずしもActiveFormでいいのか、というのはあるけどな
            PEnv.ActiveForm.Warning(PEnv.Strings.GetString("Message.CygwinUtil.KeyNotFound"));
            return String.Empty;
        }

        private static string GetCygwinRootDirectory(RegistryKey baseKey, bool check64BitHive) {
            string software = check64BitHive ? "SOFTWARE\\Wow6432Node" : "SOFTWARE";
            string[][] keyValueNameArray = new string[][] {
                new string[] { software + "\\Cygnus Solutions\\Cygwin\\mounts v2\\/", "native" },
                new string[] { software + "\\Cygwin\\setup", "rootdir" }
            };

            foreach (string[] keyValueName in keyValueNameArray) {
                RegistryKey subKey = baseKey.OpenSubKey(keyValueName[0]);
                if (subKey != null) {
                    try {
                        string val = subKey.GetValue(keyValueName[1]) as string;
                        if (val != null)
                            return val;
                    }
                    finally {
                        subKey.Close();
                    }
                }
            }

            return null;
        }
    }
}
