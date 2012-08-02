/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: MacroModule.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Windows.Forms;

using Poderosa.Config;
using Poderosa.Communication;
using Poderosa.Connection;
using Poderosa.UI;

namespace Poderosa.MacroEnv
{
	internal enum MacroType {
		Unknown,
		JavaScript,
		Assembly
	}

	internal interface IMacroEventListener {
		void IndicateMacroStarted();
		void IndicateMacroFinished();
	}

	internal class MacroModule : ICloneable {
		private MacroType _type;
		private string _path;
		private string _title;
		private string[] _additionalAssemblies;
		private bool _debugMode;
		private int _index;
		
		public MacroModule(int index) {
			_index = index;
			_additionalAssemblies = new string[0];
		}
		public int Index {
			get {
				return _index;
			}
			set {
				_index = value;
			}
		}

		public object Clone() {
			MacroModule m = new MacroModule(_index);
			m._type = _type;
			m._path = _path;
			m._title = _title;
			m._additionalAssemblies = _additionalAssemblies;
			m._debugMode = _debugMode;
			return m;
		}

		public MacroType Type {
			get {
				return _type;
			}
		}
		public string Path {
			get {
				return _path;
			}
			set {
				_path = value;
				string t = System.IO.Path.GetExtension(_path).ToLower();
				if(t.EndsWith("js"))
					_type = MacroType.JavaScript;
				else if(t.EndsWith("exe") || t.EndsWith("dll"))
					_type = MacroType.Assembly;
				else
					_type = MacroType.Unknown;
			}
		}
		public string Title {
			get {
				return _title;
			}
			set {
				_title = value;
			}
		}
		public CID CommandID {
			get {
				return CID.ExecMacro +_index;
			}
		}
		public Keys ShortCut {
			get {
				Commands.Entry e = GApp.Options.Commands.FindMacroEntry(this.Index);
				return e==null? Keys.None : (e.Modifiers | e.Key);
			}
		}


		public string[] AdditionalAssemblies {
			get {
				return _additionalAssemblies;
			}
			set {
				_additionalAssemblies = value;
			}
		}
		public bool DebugMode {
			get {
				return _debugMode;
			}
			set {
				_debugMode = value;
			}
		}

		public void Load(ConfigNode sec) {
			Path = sec["path"];
			_title = sec["title"];
			_debugMode = GUtil.ParseBool(sec["debug"], false);
			Keys shortcut = Keys.None;
			string t = sec["shortcut"];
			if(t!=null) shortcut = GUtil.ParseKey(t.Split(','));
			GApp.Options.Commands.AddEntry(new Commands.MacroEntry(_title, shortcut & Keys.Modifiers, shortcut & Keys.KeyCode, _index));
			_additionalAssemblies = sec["additional-assemblies"].Split(',');
		}
		public void Save(ConfigNode parent) {
			ConfigNode node = new ConfigNode("module");
			node["path"] = _path;
			node["title"] = _title;
			node["debug"] = _debugMode.ToString();
			Commands.Entry e = GApp.Options.Commands.FindMacroEntry(this.Index);
			if(e!=null)
				node["shortcut"] = UILibUtil.KeyString(e.Modifiers, e.Key, ',');
			node["additional-assemblies"] = Concat(_additionalAssemblies);
			parent.AddChild(node);
		}
		private string Concat(string[] v) {
			if(v==null) return "";
			StringBuilder b = new StringBuilder();
			foreach(string t in v) {
				if(b.Length>0) b.Append(';');
				b.Append(t);
			}
			return b.ToString();
		}
	}

	internal class MacroManager {
		private ArrayList _entries;
		private Hashtable _environmentVariables;
		private MacroExecutor _runningMacro;

		private IMacroEventListener _macroListener;

		public MacroManager() {
			_entries = new ArrayList();
			_environmentVariables = new Hashtable();
		}
		public IEnumerable Modules {
			get {
				return _entries;
			}
		}
		public int ModuleCount {
			get {
				return _entries.Count;
			}
		}

		public IDictionaryEnumerator EnvironmentVariables {
			get {
				return _environmentVariables.GetEnumerator();
			}
		}
		public string GetVariable(string name, string defaultvalue) {
			object t = _environmentVariables[name];
			return t==null? defaultvalue : (string)t;
		}
		public void ResetEnvironmentVariables(Hashtable newmap) {
			_environmentVariables = newmap;
		}

		public bool MacroIsRunning {
			get {
				return _runningMacro!=null;
			}
		}
		public MacroModule CurrentMacro {
			get {
				return _runningMacro.Module;
			}
		}
		public void SetMacroEventListener(IMacroEventListener f) {
			_macroListener = f;
		}


		public void Execute(IWin32Window parent, MacroModule mod) {
			if(_runningMacro!=null) {
				GUtil.Warning(parent, GApp.Strings.GetString("Message.MacroModule.AlreadyRunning"));
				return;
			}

			if(mod.Type==MacroType.Unknown) {
				GUtil.Warning(parent, GApp.Strings.GetString("Message.MacroModule.UnknownModuleType"));
				return;
			}
			else {
				try {
					GApp.Frame.Cursor = Cursors.WaitCursor;
					Assembly asm = MacroUtil.LoadMacroAssembly(mod);
					MethodInfo ep = asm.EntryPoint;
					if(ep==null)
						throw new Exception(GApp.Strings.GetString("Message.MacroModule.NoEntryPoint"));

					_runningMacro = new MacroExecutor(mod, ep);
					IndicateMacroStarted();
					_runningMacro.AsyncExec();
				}
				catch(Exception ex) {
					GUtil.Warning(parent, ex.Message);
				}
				finally {
					GApp.Frame.Cursor = Cursors.Default;
				}
			}
		}

		public void StopMacro() {
			if(_runningMacro==null) return;
			_runningMacro.Abort();
		}

		public MacroModule GetModule(int index) {
			return (MacroModule)_entries[index];
		}

		public void AddModule(MacroModule mod) {
			_entries.Add(mod);
		}
		public void RemoveModule(MacroModule mod) {
			_entries.Remove(mod);
		}
		public void RemoveAt(int n) {
			_entries.RemoveAt(n);
		}
		public void InsertModule(int n, MacroModule mod) {
			_entries.Insert(n, mod);
		}
		public void ReplaceModule(MacroModule old, MacroModule module) {
			int i = _entries.IndexOf(old);
			Debug.Assert(i!=-1);
			_entries[i] = module;
		}

		//開始はExecuteの中から呼ばれ、終了はInterThreadUIService経由で通知される
		private void IndicateMacroStarted() {
			GApp.Frame.RefreshConnection(GEnv.Connections.ActiveTag);
			GApp.Frame.MenuMacroStop.Enabled = true;
			if(_macroListener!=null)
				_macroListener.IndicateMacroStarted();
		}
		public void IndicateMacroFinished() {
			_runningMacro = null;
			GApp.Frame.RefreshConnection(GEnv.Connections.ActiveTag);
			GApp.Frame.MenuMacroStop.Enabled = false;
			if(_macroListener!=null)
				_macroListener.IndicateMacroFinished();

			foreach(ConnectionTag ct in GEnv.Connections)
				ct.Terminal.ClearMacroBuffer();
		}

		public void Load(ConfigNode parent) {
			ConfigNode node = parent.FindChildConfigNode("macro");
			if(node==null)
				SetDefault();
			else {
				ConfigNode var = node.FindChildConfigNode("variables");
				if(var!=null) 
					_environmentVariables = var.InnerHashtable;

				foreach(ConfigNode ch in node.Children) {
					if(ch.Name=="module") {
						MacroModule m = new MacroModule(_entries.Count);
						m.Load(ch);
						_entries.Add(m);
					}
				}
			}
		}
		public void SetDefault() {
			InitSample();
		}

		public void Save(ConfigNode parent) {
			ConfigNode node = new ConfigNode("macro");
			if(_environmentVariables.Count>0) {
				ConfigNode variables = new ConfigNode("variables");
				IDictionaryEnumerator de = _environmentVariables.GetEnumerator();
				while(de.MoveNext())
					variables[(string)de.Key] = (string)de.Value;
				node.AddChild(variables);
			}

			foreach(MacroModule mod in _entries)
				mod.Save(node);

			parent.AddChild(node);
		}


		private void InitSample() {
			string b = AppDomain.CurrentDomain.BaseDirectory + "macrosample\\";
			MacroModule hello = new MacroModule(0);
			hello.Title = GApp.Strings.GetString("Caption.MacroModule.SampleTitleHelloWorld");
			hello.Path = b + "helloworld.js";
			GApp.Options.Commands.AddEntry(new Commands.MacroEntry(hello.Title, Keys.None, Keys.None, hello.Index));
			_entries.Add(hello);
			MacroModule telnet = new MacroModule(1);
			telnet.Title = GApp.Strings.GetString("Caption.MacroModule.SampleTitleAutoTelnet");
			telnet.Path = b + "telnet.js";
			GApp.Options.Commands.AddEntry(new Commands.MacroEntry(telnet.Title, Keys.None, Keys.None, telnet.Index));
			_entries.Add(telnet);
			MacroModule bashrc = new MacroModule(2);
			bashrc.Title = GApp.Strings.GetString("Caption.MacroModule.SampleTitleOpenBashrc");
			bashrc.Path = b + "bashrc.js";
			GApp.Options.Commands.AddEntry(new Commands.MacroEntry(bashrc.Title, Keys.None, Keys.None, bashrc.Index));
			_entries.Add(bashrc);
		}

		public void ReloadLanguage() {
			string b = AppDomain.CurrentDomain.BaseDirectory + "macrosample\\";
			string helloworld = b + "helloworld.js";
			string autotelnet = b + "telnet.js";
			string openbashrc = b + "bashrc.js";
			foreach(MacroModule mod in _entries) {
				if(mod.Path==helloworld)
					mod.Title = GApp.Strings.GetString("Caption.MacroModule.SampleTitleHelloWorld");
				else if(mod.Path==autotelnet)
					mod.Title = GApp.Strings.GetString("Caption.MacroModule.SampleTitleAutoTelnet");
				else if(mod.Path==openbashrc)
					mod.Title = GApp.Strings.GetString("Caption.MacroModule.SampleTitleOpenBashrc");
			}
		}
	}

}
