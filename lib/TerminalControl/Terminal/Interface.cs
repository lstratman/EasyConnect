/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Interface.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Connection;
using Poderosa.Config;
using Poderosa.Text;
using Poderosa.Communication;
using Poderosa.Terminal;

namespace Poderosa
{
	public interface IPoderosaTerminalPane {
		void ApplyOptions(CommonOptions opt);
		Control AsControl();
		void SplitterDragging(int width, int height);
		void Attach(ConnectionTag tag);
		void Detach();
		void ToggleFreeSelectionMode();
		void ToggleAutoSelectionMode();
		void ApplyRenderProfile(RenderProfile prof);
		bool InFreeSelectionMode { get; }
		bool InAutoSelectionMode { get; }
		void ExitTextSelection();
		TerminalConnection Connection { get; }
	}

	internal interface IInternalTerminalPane {
		bool FakeVisible { get; set; }

		//TerminalDocument Document { get; }
		void DataArrived();

	}

	public interface IPoderosaContainer : IWin32Window {
		void RefreshConnection(ConnectionTag ct);
		void RemoveConnection(ConnectionTag ct);
		void ActivateConnection(ConnectionTag ct);
		Form AsForm();
		Size TerminalSizeForNextConnection 
        { get; } //MultiPaneContainerÇ…ì]ëó
		int  PositionForNextConnection { get; } //è„Ç∆Ç‹Ç∆ÇﬂÇΩÇ¢
		void ShowContextMenu(Point pt, ConnectionTag ct);
		void IndicateBell();
		void SetStatusBarText(string text);
		void SetSelectionStatus(SelectionStatus status);
		bool MacroIsRunning { get; } 
		void OnDragEnter(DragEventArgs args);
		void OnDragDrop(DragEventArgs args);
		bool IgnoreErrors { get; } 

		CommandResult ProcessShortcutKey(Keys key);
	}

	public enum SelectionStatus {
		None,
		Free,
		Auto
	}
}
