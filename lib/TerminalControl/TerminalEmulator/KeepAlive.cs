/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: KeepAlive.cs,v 1.3 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Sessions;

namespace Poderosa.Terminal {
    internal class KeepAlive {
        private Timer _timer;
        private int _prevInterval;

        public KeepAlive() {
            _prevInterval = 0;
        }

        public void Refresh(int interval) {
            bool first = false;
            if (_timer == null) {
                first = true;
                _timer = new Timer();
                _timer.Tick += new EventHandler(OnTimer);
            }

            if (!first && _prevInterval == interval)
                return; //既存設定と変更のない場合は何もしない

            if (interval > 0) {
                _timer.Interval = interval;
                _timer.Start();
            }
            else
                _timer.Stop();

            _prevInterval = interval;
        }


        private void OnTimer(object sender, EventArgs args) {
            //TODO アプリケーション内部イベントログでも作ってこういうのは記録していくのがいいのか？
            foreach (ISession s in TerminalEmulatorPlugin.Instance.GetSessionManager().AllSessions) {
                IAbstractTerminalHost ts = (IAbstractTerminalHost)s.GetAdapter(typeof(IAbstractTerminalHost));
                if (ts != null && ts.TerminalConnection != null && ts.TerminalConnection.TerminalOutput != null) {
                    ts.TerminalConnection.TerminalOutput.SendKeepAliveData();
                }
            }
        }


    }
}
