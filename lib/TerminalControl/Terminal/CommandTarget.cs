/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: CommandTarget.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Poderosa.Terminal;
using Poderosa.Connection;
using Poderosa.Communication;
using Poderosa.Text;
using Poderosa.Config;
using Poderosa.Forms;
using Poderosa.Toolkit;

namespace Poderosa {
	public enum CommandResult {
		Success,
		Failed,
		Denied,
		Cancelled,
		Ignored,
		NOP
	}

	public class GlobalCommandTarget {
		public GlobalCommandTarget() {
		}

		public CommandResult Copy() {
			if(!GEnv.TextSelection.IsEmpty) {
				string t = GEnv.TextSelection.GetSelectedText();
				if(t.Length > 0)
					CopyToClipboard(t);
				return CommandResult.Success;
			}
			else
				return CommandResult.Ignored;
		}
		public CommandResult CopyAsLook() {
			if(!GEnv.TextSelection.IsEmpty) {
				string t = GEnv.TextSelection.GetSelectedTextAsLook();
				if(t.Length > 0)
					CopyToClipboard(t);
				return CommandResult.Success;
			}
			else
				return CommandResult.Ignored;
		}
		private void CopyToClipboard(string data) {
			try {
				Clipboard.SetDataObject(data, false);
			}
			catch(Exception ex) {
				//きわめて稀にだがここで失敗するらしい
				GUtil.Warning(GEnv.Frame, ex.Message);
			}
		}

		public void AddNewTerminal(ConnectionTag ct) {
			GEnv.Connections.Add(ct);
			AddNewTerminalInternal(ct);
			ct.Receiver.Listen();
		}
		protected virtual void AddNewTerminalInternal(ConnectionTag ct) {
		}

		public virtual CommandResult SetConnectionLocation(ConnectionTag ct, TerminalPane pane) {
			return CommandResult.Ignored;
		}
	}


	public class ConnectionCommandTarget {
		protected TerminalConnection _connection;

		public ConnectionCommandTarget(TerminalConnection con) {
			_connection = con;

		}
		public TerminalConnection Connection {
			get {
				return _connection;
			}
		}

		public CommandResult Disconnect() {
			if(_connection.IsClosed) return CommandResult.Ignored;

			if(GEnv.Options.CloseOnDisconnect) 
				Close();
			else {
				_connection.Disconnect();
				_connection.Close();
				GEnv.Frame.RefreshConnection(GEnv.Connections.FindTag(_connection));
			}
			return CommandResult.Success;
		}
		public CommandResult Close() {
			try {
				_connection.Disconnect();
				_connection.Close();
			}
			catch(Exception ex) {
				//ここでエラーが発生しても処理は続行してアプリ自体は実行を継続
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.ConnectionCommandTarget.CloseError")+ex.Message);
			}
			bool active = _connection==GEnv.Connections.ActiveConnection;
			ConnectionTag ct = GEnv.Connections.FindTag(_connection);
			//Debug.WriteLine("ct==null? " + (ct==null));
			if(ct==null) return CommandResult.Ignored;

			TerminalPane pane = ct.AttachedPane;
			ct.IsTerminated = true;
			GEnv.Frame.RemoveConnection(ct);
			ConnectionTag next = GEnv.Connections.GetCandidateOfActivation(ct.PositionIndex, ct);
			GEnv.Connections.Remove(_connection);
			if(next!=null) {
				if(pane!=null) pane.Attach(next);
				if(active)
					GEnv.Frame.ActivateConnection(next); //もともとアクティブでない接続が切れたときは変更しない
			}
			else {
				if(pane!=null) {
					pane.FakeVisible = false;
					pane.Detach();
				}
				GEnv.Frame.RefreshConnection(null);
			}
			
			if(GEnv.Options.QuitAppWithLastPane && GEnv.Connections.Count==0)
				GEnv.Frame.AsForm().Close();

			return CommandResult.Success;
		}
		public CommandResult Resize(int width, int height) {
			if(_connection.IsClosed) return CommandResult.Ignored;
            // this line was aborting up the resize event
            // What is it for?
			//if(GEnv.Connections.FindTag(_connection).ModalTerminalTask!=null) return CommandResult.Denied;
			_connection.TextLogger.TerminalResized(width, height);
			_connection.Resize(width, height);
			return CommandResult.Success;
		}

		public CommandResult Focus() {
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			GEnv.Frame.ActivateConnection(tag);
			if(tag.Pane==null)
				return CommandResult.Failed;
			else {
				tag.Pane.AsControl().Focus();
				return CommandResult.Success;
			}
		}

		public CommandResult ClearScreen() {
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			TerminalDocument doc = tag.Document;
			lock(doc) {
				GLine l = doc.TopLine;
				int top_id = l.ID;
				int limit = l.ID + _connection.TerminalHeight;
				while(l!=null && l.ID<limit) {
					l.Clear();
					l = l.NextLine;
				}
				doc.CurrentLineNumber = top_id;
				doc.CaretColumn = 0;
				doc.InvalidateAll();
				if(tag.Pane!=null) {
					GEnv.TextSelection.Clear();
					tag.Pane.Invalidate();
				}
			}
			return CommandResult.Success;
		}
		public CommandResult ClearBuffer() {
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			TerminalDocument doc = tag.Document;
			lock(doc) {
				doc.Clear();
				tag.Receiver.AdjustTransientScrollBar();
				if(tag.Pane!=null) {
					GEnv.TextSelection.Clear();
					tag.Pane.CommitTransientScrollBar();
					tag.Pane.Invalidate();
				}
			}
			return CommandResult.Success;
		}

		public CommandResult SelectAll() {
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			GEnv.TextSelection.SelectAll(tag.Pane);
			tag.Pane.AsControl().Invalidate();
			return CommandResult.Success;
		}

		public CommandResult ToggleFreeSelectionMode() {
			TerminalPane p = GEnv.Connections.FindTag(_connection).Pane;
			if(p==null) return CommandResult.Ignored;
			p.ToggleFreeSelectionMode();
			return CommandResult.Success;
		}
		public CommandResult AreYouThere() {
			if(_connection.IsClosed) return CommandResult.Ignored;
			if(GEnv.Connections.FindTag(_connection).ModalTerminalTask!=null) return CommandResult.Denied;

			try {
				_connection.AreYouThere();
				return CommandResult.Success;
			}
			catch(NotSupportedException) {
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.ConnectionCommandTarget.AreYouThereCondition"));
				return CommandResult.Failed;
			}
		}
		public CommandResult SendBreak() {
			if(_connection.IsClosed) return CommandResult.Ignored;
			if(GEnv.Connections.FindTag(_connection).ModalTerminalTask!=null) return CommandResult.Denied;

			try {
				_connection.SendBreak();
				return CommandResult.Success;
			}
			catch(NotSupportedException) {
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.ConnectionCommandTarget.BreakCondition"));
				return CommandResult.Failed;
			}
		}


		public CommandResult ResetTerminal() {
			GEnv.Connections.FindTag(_connection).Terminal.FullReset();
			return CommandResult.Success;
		}

		public virtual CommandResult Paste() {
			string value = (string)Clipboard.GetDataObject().GetData("Text");
			if(value==null || value.Length==0) return CommandResult.Ignored;
			
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			if(tag.ModalTerminalTask!=null) return CommandResult.Denied;
			return PasteMain(value);
		}

		protected CommandResult PasteMain(string value) {
			ConnectionTag tag = GEnv.Connections.FindTag(_connection);
			PasteProcessor p = new PasteProcessor(tag, value);
			
			try {
				p.Perform();
				return CommandResult.Success;
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.StackTrace);
				GUtil.Warning(GEnv.Frame, GEnv.Strings.GetString("Message.ConnectionCommandTarget.SendError") + ex.Message);
				return CommandResult.Failed;
			}
		}

		public CommandResult DumpText() {
			GEnv.Connections.FindTag(_connection).Document.Dump("COMMAND");
			return CommandResult.Success;
		}
	}
	
}
