using System;
using System.ComponentModel;
using System.Windows.Forms;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Base class from which all forms implementing a UI for remote connection protocols will inherit.
	/// </summary>
	public abstract partial class BaseConnectionForm : Form, IConnectionForm
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		protected BaseConnectionForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Returns the control element in which the UI for the connection is hosted.
		/// </summary>
		protected abstract Control ConnectionWindow
		{
			get;
		}

		/// <summary>
		/// Flag indicating whether the connection has been established yet.
		/// </summary>
		public bool IsConnected
		{
			get;
			protected set;
		}

		protected bool IsClosing
		{
			get;
			set;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			IsClosing = true;
			base.OnClosing(e);
		}

		/// <summary>
		/// Handler method that's called when the form is loaded initially.  Wires up the <see cref="OnConnectionWindowGotFocus"/> handler to 
		/// <see cref="Control.GotFocus"/> on <see cref="ConnectionWindow"/>.
		/// </summary>
		/// <param name="e">Arguments associated with this event.</param>
		protected override void OnLoad(EventArgs e)
		{
			ConnectionWindow.GotFocus += OnConnectionWindowGotFocus;
			base.OnLoad(e);
		}

		/// <summary>
		/// Handler method that's called when the UI control for the connection gains focus.
		/// </summary>
		/// <param name="sender">Object from which this event originated, <see cref="ConnectionWindow"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		protected virtual void OnConnectionWindowGotFocus(object sender, EventArgs e)
		{
			if (ConnectionFormFocused != null)
				ConnectionFormFocused(ConnectionWindow, e);
		}

		/// <summary>
		/// Handler method that's called when the connection is established.  Sets <see cref="IsConnected"/> to true and calls the <see cref="Connected"/>
		/// event.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		protected virtual void OnConnected(object sender, EventArgs e)
		{
			IsConnected = true;
			connectingLabel.Visible = false;
			ConnectionWindow.Visible = true;

			if (Connected != null)
				Connected(sender, e);
		}

		/// <summary>
		/// Handler method that's called when the connection is lost.  Sets <see cref="IsConnected"/> to false and calls the <see cref="LoggedOff"/>
		/// event.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		protected virtual void OnLoggedOff(object sender, EventArgs e)
		{
			if (IsClosing)
				return;

			IsConnected = false;
			ConnectionWindow.Visible = false;

			if (LoggedOff != null)
				LoggedOff(sender, e);

			IsClosing = true;
			ParentForm.Invoke(new Action(() => ParentForm.Close()));
		}

		/// <summary>
		/// Handler method that's called when the connection is lost.  Sets <see cref="IsConnected"/> to false and calls the <see cref="ConnectionLost"/>
		/// event.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		protected virtual void OnConnectionLost(object sender, EventArgs e)
		{
			if (IsClosing)
				return;

			IsConnected = false;
			ConnectionWindow.Visible = false;
			disconnectedPanel.Visible = true;

			if (ConnectionLost != null)
				ConnectionLost(sender, e);
		}

		/// <summary>
		/// Event that is fired when the connection for this window is established.
		/// </summary>
		public event EventHandler Connected;

		/// <summary>
		/// Event that is fired when the connection for this window is lost.
		/// </summary>
		public event EventHandler LoggedOff;

		/// <summary>
		/// Event that is fired when the connection for this window is lost.
		/// </summary>
		public event EventHandler ConnectionLost;

		/// <summary>
		/// Event that is fired when <see cref="ConnectionWindow"/> gains focus.
		/// </summary>
		public event EventHandler ConnectionFormFocused;

		/// <summary>
		/// Abstract method to be implemented by the inheriting classes to actually establish the connection to the remote system.
		/// </summary>
		public abstract void Connect();

		/// <summary>
		/// Event handler that is called when <see cref="reconnectButton"/> is clicked; re-establishes the current connection.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void reconnectButton_Click(object sender, EventArgs e)
		{
			disconnectedPanel.Visible = false;
			connectingLabel.Visible = true;

			Connect();
		}
	}
}