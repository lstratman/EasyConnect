/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: KeepAlive.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Threading;
using System.Diagnostics;

using Poderosa.Connection;

namespace Poderosa.Terminal
{
	internal class KeepAlive
	{
		public KeepAlive() {
		}

		public void SetTimerToConnectionTag(ConnectionTag ct) {
			if(GEnv.Options.KeepAliveInterval==0) {
				if(ct.Timer!=null) {
					ct.Timer.Dispose();
					ct.Timer = null;
				}
			}
			else {
				if(ct.Timer==null)
					ct.Timer = new Timer(new TimerCallback(OnTimer), ct, GEnv.Options.KeepAliveInterval, Timeout.Infinite);
				else
					ct.Timer.Change(GEnv.Options.KeepAliveInterval, Timeout.Infinite);
			}
		}
		public void SetTimerToAllConnectionTags() {
			foreach(ConnectionTag ct in GEnv.Connections)
				SetTimerToConnectionTag(ct);
		}
		public void ClearTimerToConnectionTag(ConnectionTag ct) {
			if(ct.Timer!=null) {
				ct.Timer.Dispose();
				ct.Timer = null;
			}
		}

		private static void OnTimer(object state) {
			ConnectionTag ct = (ConnectionTag)state;
			if(!ct.Connection.IsClosed) {
				//Debug.WriteLine("Send KA " + ct.Button.Text + ";"+DateTime.Now.ToString());
				ct.Connection.SendKeepAliveData();
				ct.Timer.Change(GEnv.Options.KeepAliveInterval, Timeout.Infinite);
			}
		}

	}
}
