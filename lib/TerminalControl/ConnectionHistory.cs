/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ConnectionHistory.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

using Poderosa.ConnectionParam;
using Poderosa.Toolkit;
using Poderosa.Communication;
using Granados.SSHC;

namespace Poderosa.Config {
	/// <summary>
	/// 過去の接続履歴の保持。シリアライズ機能も含む。
	/// </summary>
	internal class ConnectionHistory : IEnumerable {	

		protected ArrayList _history; //TCPTerminalParamのコレクション

		public ConnectionHistory() {
			_history = new ArrayList();
		}

		public IEnumerator GetEnumerator() {
			return _history.GetEnumerator();
		}
		public void Clear() {
			_history.Clear();
		}
		public void Append(TerminalParam tp) {
			_history.Add(tp);
		}

		public int Count {
			get {
				return _history.Count;
			}
		}
		public TCPTerminalParam TopTCPParam {
			get {
				foreach(TerminalParam p in _history) {
					TCPTerminalParam tp = p as TCPTerminalParam;
					if(tp!=null) return tp;
				}
				return new TelnetTerminalParam("");
			}
		}
		public SerialTerminalParam TopSerialParam {
			get {
				foreach(TerminalParam p in _history) {
					SerialTerminalParam tp = p as SerialTerminalParam;
					if(tp!=null) return tp;
				}
				return new SerialTerminalParam();
			}
		}
		public CygwinTerminalParam TopCygwinParam {
			get {
				foreach(TerminalParam p in _history) {
					CygwinTerminalParam tp = p as CygwinTerminalParam;
					if(tp!=null) return tp;
				}
				return new CygwinTerminalParam();
			}
		}
		public SFUTerminalParam TopSFUParam {
			get {
				foreach(TerminalParam p in _history) {
					SFUTerminalParam tp = p as SFUTerminalParam;
					if(tp!=null) return tp;
				}
				return new SFUTerminalParam();
			}
		}

		public TCPTerminalParam SearchByHost(string host) {
			foreach(TerminalParam p in _history) {
				TCPTerminalParam tp = p as TCPTerminalParam;
				if(tp!=null && tp.Host==host) return tp;
			}
			return null;
		}

		public void LimitCount(int count) {
			if(_history.Count > count) _history.RemoveRange(count, _history.Count-count);
		}

		//最新のMRUリストに更新
		public void Update(TerminalParam newparam_) {
			int n = 0;
			TerminalParam newparam = (TerminalParam)newparam_.Clone();
			newparam.LogPath = "";
			newparam.LogType = LogType.None;
			foreach(TerminalParam p in _history) {
				if(p.Equals(newparam)) {
					_history.RemoveAt(n);
					_history.Insert(0, newparam);
					return;
				}
				n++;
			}

			_history.Insert(0, newparam);
			//ランタイムに出てくる候補数は無制限にする
			if(_history.Count > 100)
				_history.RemoveRange(GApp.Options.MRUSize, _history.Count-100);
		}
		public void ReplaceIdenticalParam(TerminalParam newparam_) {
			int n = 0;
			TerminalParam newparam = (TerminalParam)newparam_.Clone();
			newparam.LogPath = "";
			newparam.LogType = LogType.None;
			foreach(TerminalParam p in _history) {
				if(p.Equals(newparam)) {
					_history[n] = newparam;
					return;
				}
				n++;
			}
		}
		public void Save(ConfigNode parent) {
			LimitCount(GApp.Options.MRUSize);

			ConfigNode node = new ConfigNode("connection-history");
			foreach(TerminalParam p in _history) {
				ConfigNode con = new ConfigNode("connection");
				p.Export(con);
				node.AddChild(con);
			}
			parent.AddChild(node);
		}
		public void Load(ConfigNode parent) {
			ConfigNode node = parent.FindChildConfigNode("connection-history");
			if(node!=null) {
				foreach(ConfigNode ch in node.Children) {
					_history.Add(TerminalParam.CreateFromConfigNode(ch));
				}
			}
		}


		private delegate string StrProp(TerminalParam p);
		private delegate int    IntProp(TerminalParam p);

		private string ReturnHost(TerminalParam p) {
			TCPTerminalParam pp = p as TCPTerminalParam;
			return pp==null? null : pp.Host;
		}
		private int    ReturnPort(TerminalParam p) {
			TCPTerminalParam pp = p as TCPTerminalParam;
			return pp==null? -1 : pp.Port;
		}
		private string ReturnAccount(TerminalParam p) {
			SSHTerminalParam pp = p as SSHTerminalParam;
			return pp==null? null : pp.Account;
		}
		private string ReturnLogPath(TerminalParam p) {
			return p.LogPath;
		}

		private StringCollection CollectString(StrProp prop) {
			StringCollection result = new StringCollection();
			foreach(TerminalParam param in _history) {
				string t  = prop(param);
				if(t!=null && t.Length>0 && !result.Contains(t)) result.Add(t);
			}
			return result;
		}
		private int[] CollectInt(IntProp prop, ArrayList result) {
			foreach(TerminalParam param in _history) {
				int t  = prop(param);
				if(t>0 && !result.Contains(t)) result.Add(t);
			}
			return (int[])result.ToArray(typeof(int));
		}

		//TCPTerminalParam各要素ごとのコレクション
		public StringCollection Hosts {
			get {
				return CollectString(new StrProp(this.ReturnHost));
			}
		}
		public int[] Ports {
			get {
				ArrayList a = new ArrayList();
				a.Add(23); a.Add(22); //Telnetを先に表示する
				return CollectInt(new IntProp(this.ReturnPort), a);
			}
		}
		public StringCollection Accounts {
			get {
				return CollectString(new StrProp(this.ReturnAccount));
			}
		}
		public StringCollection LogPaths {
			get {
				return CollectString(new StrProp(this.ReturnLogPath));
			}
		}
	}



}
