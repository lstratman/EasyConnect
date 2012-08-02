/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: KnownHosts.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Collections;
using System.IO;

using Poderosa.ConnectionParam;
using Poderosa.SSH;

namespace Poderosa.Config
{
	/// <summary>
	/// known_hostsの一覧を管理する。
	/// </summary>
	public abstract class KnownHosts {

		protected string _fileName;

		public abstract void Load(string filename);

		public abstract void Clear();

		public abstract void WriteTo(string filename);

		protected static void WriteEntry(StreamWriter w, string host, string key_expr) {
			w.Write(host);
			w.Write(' ');
			w.WriteLine(key_expr);
		}
	}

	public class SSHKnownHosts : KnownHosts, ISSHKnownHosts {
		private Hashtable _dataForSSH1; //hostからエントリへのマップ
		private Hashtable _dataForSSH2;

		public SSHKnownHosts()	{
			_dataForSSH1 = new Hashtable();
			_dataForSSH2 = new Hashtable();
		}
		public override void Clear() {
			_dataForSSH1 = new Hashtable();
			_dataForSSH2 = new Hashtable();
		}


		public override void Load(string filename) {
			_fileName = filename;
			_dataForSSH1.Clear();
			_dataForSSH2.Clear();

			StreamReader r = null;
			try {
				r = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read));
				string line = r.ReadLine();
				while(line!=null) {
					int sp = line.IndexOf(' ');
					if(sp==-1) throw new IOException("known_hosts is corrupted: host name field is not found");

					string body = line.Substring(sp+1);
					if(body.StartsWith("ssh1"))
						_dataForSSH1[line.Substring(0, sp)] = body;
					else
						_dataForSSH2[line.Substring(0, sp)] = body;

					line = r.ReadLine();
				}
			}
			finally {
				if(r!=null) r.Close();
			}
		}

		public void Update(SSHTerminalParam param, string key) {
			if(param.Method==ConnectionMethod.SSH1)
				_dataForSSH1[ToKeyString(param)] = key;
			else
				_dataForSSH2[ToKeyString(param)] = key;
		}

		public KeyCheckResult Check(SSHTerminalParam param, string key) {
			object k = param.Method==ConnectionMethod.SSH1? _dataForSSH1[ToKeyString(param)] : _dataForSSH2[ToKeyString(param)];
			if(k==null)
				return KeyCheckResult.NotExists;
			else
				return key.Equals(k)? KeyCheckResult.OK : KeyCheckResult.Different;
		}

		public override void WriteTo(string filename) {
			if(_dataForSSH1.Count==0 && _dataForSSH2.Count==0) return;

			StreamWriter w = null;
			try {
				w = new StreamWriter(File.Open(filename, FileMode.Create));
				IDictionaryEnumerator ie = _dataForSSH1.GetEnumerator();
				while(ie.MoveNext())
					WriteEntry(w, (string)ie.Key, (string)ie.Value);
				
				ie = _dataForSSH2.GetEnumerator();
				while(ie.MoveNext())
					WriteEntry(w, (string)ie.Key, (string)ie.Value);
				
			}
			finally {
				if(w!=null) w.Close();
			}
		}

		private static string ToKeyString(SSHTerminalParam param) {
			string h = param.Host;
			if(param.Port!=22) h += ":" + param.Port;
			return h;
		}
	}
}
