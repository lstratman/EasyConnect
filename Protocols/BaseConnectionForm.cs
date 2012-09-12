using System;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    /// <summary>
    /// Base class from which all forms implementing a UI for remote connection protocols will inherit.
    /// </summary>
    public abstract class BaseConnectionForm : Form, IConnectionForm
    {
        /// <summary>
        /// Default constructor, sets <see cref="CloseParentFormOnDisconnect"/> to true.
        /// </summary>
        protected BaseConnectionForm()
        {
            CloseParentFormOnDisconnect = true;
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
        /// Flag indicating if the parent window should be closed when the connection is terminated.
        /// </summary>
        public bool CloseParentFormOnDisconnect
        {
            get;
            set;
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

            if (Connected != null)
                Connected(sender, e);
        }

        /// <summary>
        /// Event that is fired when the connection for this window is established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Event that is fired when <see cref="ConnectionWindow"/> gains focus.
        /// </summary>
        public event EventHandler ConnectionFormFocused;

        /// <summary>
        /// Abstract method to be implemented by the inheriting classes to actually establish the connection to the remote system.
        /// </summary>
        public abstract void Connect();
    }

    /// <summary>
    /// Version of <see cref="BaseConnectionForm"/> that contains a strongly typed <see cref="IConnection"/> property.
    /// </summary>
    /// <typeparam name="T">Type of connection that will be established by this window.</typeparam>
    public abstract class BaseConnectionForm<T> : BaseConnectionForm, IConnectionForm<T>
        where T : IConnection
    {
        /// <summary>
        /// Connection that will be established.
        /// </summary>
        public T Connection
        {
            get;
            set;
        }
    }
}