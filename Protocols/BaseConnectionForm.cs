using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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

		/// <summary>
		/// Flag indicating whether the form is in the process of closing.
		/// </summary>
		protected bool IsClosing
		{
			get;
			set;
		}

		/// <summary>
		/// Error, if any, that was encountered when connecting to the remote server.
		/// </summary>
		public string ConnectionErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		/// Handler method that's called when the form has started closing.  Sets the <see cref="IsClosing"/> property to true.
		/// </summary>
		/// <param name="e">Arguments associated with this event.</param>
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
		/// Handler method that's called when the form's size is changed.  Updates the location for <see cref="disconnectedPanel"/> to keep it centered in the
		/// screen.
		/// </summary>
		/// <param name="e">Arguments associated with this event.</param>
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			disconnectedPanel.Location = new Point(
				Convert.ToInt32(Math.Round((Width - disconnectedPanel.Width) / (double) 2)), Convert.ToInt32(Math.Round((Height - disconnectedPanel.Height) / (double) 2)));
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
			Invoke(
				new Action(
					() =>
						{
							IsConnected = true;
							connectingLabel.Visible = false;
							ConnectionWindow.Visible = true;
						}));

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
			if (IsClosing || Parent == null)
				return;

			Invoke(
				new Action(
					() =>
						{
							IsConnected = false;
							ConnectionWindow.Visible = false;
							connectingLabel.Visible = false;
						}));

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
			if (IsClosing || Parent == null)
				return;

			Invoke(
				new Action(
					() =>
						{
							ConnectionWindow.Visible = false;
							disconnectedPanel.Visible = true;
							disconnectedLabel.Text = IsConnected
								                         ? "Disconnected"
								                         : "Unable to connect";

							Exception exception = e is ErrorEventArgs
								                      ? (e as ErrorEventArgs).GetException()
								                      : null;

							_errorMessageLinkLabel.Visible = exception != null && !String.IsNullOrEmpty(exception.Message);
							ConnectionErrorMessage = exception == null
								                         ? null
								                         : exception.Message.Replace("\r", "").Replace("\n", "\r\n");
							_textLayoutPanel.Width = _errorMessageLinkLabel.Visible
								                         ? _errorMessageLinkLabel.Width + disconnectedLabel.Width
								                         : disconnectedLabel.Width;
							_textLayoutPanel.Location = new Point(
								Convert.ToInt32(Math.Round((disconnectedPanel.Width - _textLayoutPanel.Width) / (double) 2)), _textLayoutPanel.Location.Y);

							connectingLabel.Visible = false;
							IsConnected = false;
						}));

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

		/// <summary>
		/// Event handler that is called when <see cref="_errorMessageLinkLabel"/> is clicked; displays the contents of <see cref="ConnectionErrorMessage"/> to
		/// the user.
		/// </summary>
		/// <param name="sender">Object from which this event originated.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _errorMessageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ErrorMessageDisplayForm errorMessageWindow = new ErrorMessageDisplayForm
				                                             {
					                                             ErrorMessage = ConnectionErrorMessage
				                                             };
			errorMessageWindow.ShowDialog(this);
		}
	}
}