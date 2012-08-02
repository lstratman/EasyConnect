/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: MacroExec.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Threading;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Microsoft.JScript;
using System.CodeDom.Compiler;

using MacroEnvironment = Poderosa.Macro.Environment;

namespace Poderosa.MacroEnv
{
	internal class MacroUtil
	{
		public static Assembly LoadMacroAssembly(MacroModule mod) {
			if(mod.Type==MacroType.Assembly) {
				return Assembly.LoadFrom(mod.Path);
			}
			else if(mod.Type==MacroType.JavaScript) {
				ICodeCompiler compiler = new JScriptCodeProvider().CreateCompiler();
				CompilerParameters param = new CompilerParameters();
				param.CompilerOptions += "/debug";
				param.GenerateInMemory = true;
				param.GenerateExecutable = true;
				//param.ReferencedAssemblies.Add("mscorlib"); //要るの？
				param.ReferencedAssemblies.Add("System.Drawing.dll");
				param.ReferencedAssemblies.Add(GetMyExePath());
				param.ReferencedAssemblies.Add(GetGTerminalPath());
				foreach(string x in mod.AdditionalAssemblies)
					if(x.Length>0) param.ReferencedAssemblies.Add(x);

				CompilerResults result = compiler.CompileAssemblyFromFile(param, mod.Path);
				if(result.Errors.Count>0) {
					StringBuilder bld = new StringBuilder();
					bld.Append(GApp.Strings.GetString("Message.MacroExec.FailedToCompileScript"));
					foreach(CompilerError err in result.Errors) {
						bld.Append(String.Format("Line {0} Column {1} : {2}\n", err.Line, err.Column, err.ErrorText));
					}
					throw new Exception(bld.ToString());
				}

				return result.CompiledAssembly;
			}
			else
				throw new Exception("Unsupported macro module type " + mod.Type.ToString() + " is specified.");
		}
		private static string GetMyExePath() {
			string t = typeof(MacroUtil).Assembly.CodeBase;
			int c1 = t.IndexOf(':'); //先頭はfile://...とくる
			int c2 = t.IndexOf(':', c1+1); //これがドライブ名直後のコロン
			t = t.Substring(c2-1);
			return t.Replace('/', '\\');
		}
		private static string GetGTerminalPath() {
			string t = typeof(GEnv).Assembly.CodeBase;
			int c1 = t.IndexOf(':'); //先頭はfile://...とくる
			int c2 = t.IndexOf(':', c1+1); //これがドライブ名直後のコロン
			t = t.Substring(c2-1);
			return t.Replace('/', '\\');
		}
	}
	internal class MacroExecutor {
		
		private MacroModule _module;
		private MethodInfo _entryPoint;
		private MacroTraceWindow _traceWindow;
		private Thread _macroThread;

		public MacroExecutor(MacroModule mod, MethodInfo ep) {
			_module = mod;
			_entryPoint = ep;
			if(mod.DebugMode) {
				_traceWindow = new MacroTraceWindow();
				_traceWindow.AdjustTitle(mod);
				_traceWindow.Owner = GApp.Frame;
				_traceWindow.Show();
			}
		}
		public MacroModule Module {
			get {
				return _module;
			}
		}


		public void AsyncExec() {
			_macroThread = new Thread(new ThreadStart(MacroMain));
			_macroThread.Start();
		}

		private void MacroMain() {
			try {
				InitEnv();
				
				_entryPoint.Invoke(null, new object[1] { new string[0] });
			}
			catch(TargetInvocationException tex) {
				Exception inner = tex.InnerException;
				if(_traceWindow==null) {
					GApp.InterThreadUIService.Warning(String.Format(GApp.Strings.GetString("Message.MacroExec.ExceptionWithoutTraceWindow"), inner.Message));
					Debug.WriteLine("TargetInvocationException");
					Debug.WriteLine(inner.GetType().Name);
					Debug.WriteLine(inner.Message);
					Debug.WriteLine(inner.StackTrace);
				}
				else {
					_traceWindow.AddLine(GApp.Strings.GetString("Message.MacroExec.ExceptionInMacro"));
					_traceWindow.AddLine(String.Format("{0} : {1}", inner.GetType().FullName, inner.Message));
					_traceWindow.AddLine(inner.StackTrace);
				}
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.Message);
				Debug.WriteLine(ex.StackTrace);
			}
			finally {
				GApp.InterThreadUIService.MacroFinished();
			}
		}
		private void InitEnv() {
			MacroEnvironment.Init(new ConnectionListImpl(), new UtilImpl(), new FrameImpl(), new DebugServiceImpl(_traceWindow));
		}

		public void Abort() {
			_macroThread.Abort();
		}
			

	}
}
