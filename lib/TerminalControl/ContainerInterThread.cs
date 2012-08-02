/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ContainerInterThread.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Threading;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Communication;
using Poderosa.Text;
using Poderosa.Config;
using Poderosa.Forms;

namespace Poderosa
{
	internal class ContainerInterThreadUIService : InterThreadUIService {
		private TerminalParam _terminalParam;
		private ConnectionTag _resultConnection;
		private PanePosition _destinationPanePosition;
		private string _shortcutFile;

		private TerminalConnection _connection;

		public enum CService {
			ActivateConnection = InterThreadUIService.Service.End+1,
			SetPanePosition,
			SetFrameStyle,
			CloseConnection,
			OpenConnection,
			OpenShortcut,
			MacroFinished
		}

		//SetFrameStyle
		private GFrameStyle _style;
		private int _width1;
		private int _height1;
		private int _width2;
		private int _height2;

		public void SetPanePosition(ConnectionTag tag, PanePosition pos) {
			lock(this) {
				_connection = tag.Connection;
				_destinationPanePosition = pos;
				SendMessageCore(CService.SetPanePosition);
			}
		}

		public void SetFrameStyle(GFrameStyle style, int w1, int h1, int w2, int h2) {
			lock(this) {
				_style = style;
				_width1 = w1;
				_height1 = h1;
				_width2 = w2;
				_height2 = h2;
				SendMessageCore(CService.SetFrameStyle);
			}
		}

		public void MacroFinished() {
			lock(this) {
				SendMessageCore(CService.MacroFinished);
			}
		}
		public ConnectionTag OpenConnection(TerminalParam param) {
			lock(this) {
				_terminalParam = param;
				SendMessageCore(CService.OpenConnection);
			}
			return _resultConnection;
		}
		public ConnectionTag OpenShortcut(string filename) {
			lock(this) {
				_shortcutFile = filename;
				SendMessageCore(CService.OpenShortcut);
			}
			return _resultConnection;
		}
		public void ActivateConnection(ConnectionTag tag) {
			lock(this) {
				_connection = tag.Connection;
				SendMessageCore(CService.ActivateConnection);
			}
		}
		public void CloseConnection(ConnectionTag tag) {
			_connection = tag.Connection;
			lock(this) {
				SendMessageCore(CService.CloseConnection);
			}
		}
		private void SendMessageCore(CService svc) {
			Win32.SendMessage(_mainFrameHandle, GConst.WMG_UIMESSAGE, new IntPtr((int)svc), IntPtr.Zero);
		}

		private void SetFrameStyle() {
			GApp.GlobalCommandTarget.SetFrameStyle(_style);
			GApp.Frame.PaneContainer.ResizeByChar(_width1, _height1, _width2, _height2);
		}

		public override void ProcessMessage(IntPtr wParam, IntPtr lParam) {
			switch((CService)(int)wParam) {
				case CService.SetPanePosition:
					GEnv.Frame.ActivateConnection(GEnv.Connections.FindTag(_connection));
					if(GApp.Options.FrameStyle==GFrameStyle.DivHorizontal)
						GApp.GlobalCommandTarget.MoveActivePane(_destinationPanePosition==PanePosition.First? Keys.Up : Keys.Down);
					else if(GApp.Options.FrameStyle==GFrameStyle.DivVertical)
						GApp.GlobalCommandTarget.MoveActivePane(_destinationPanePosition==PanePosition.First? Keys.Left : Keys.Right);
					break;
				case CService.SetFrameStyle:
					SetFrameStyle();
					break;
				case CService.MacroFinished:
					GApp.MacroManager.IndicateMacroFinished();
					break;
				case CService.OpenConnection:
					_resultConnection = GApp.GlobalCommandTarget.SilentNewConnection(_terminalParam);
					break;
				case CService.OpenShortcut:
					_resultConnection = GApp.GlobalCommandTarget.SilentOpenShortCut(_shortcutFile);
					break;
				case CService.ActivateConnection:
					GApp.GlobalCommandTarget.ActivateConnection(_connection);
					break;
				case CService.CloseConnection:
					GApp.GetConnectionCommandTarget(_connection).Close();
					break;
				default:
					base.ProcessMessage(wParam, lParam);
					break;
			}
		}

	}

}
