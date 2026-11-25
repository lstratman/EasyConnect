using MarcusW.VncClient.Rendering;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using System.Drawing;
using System.Windows.Forms;
using MarcusW.VncClient;
using MarcusW.VncClient.Protocol.Implementation.MessageTypes.Outgoing;

using VncSize = MarcusW.VncClient.Size;
using VncScreen = MarcusW.VncClient.Screen;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;
using VncMouseButtons = MarcusW.VncClient.MouseButtons;
using FormsMouseButtons = System.Windows.Forms.MouseButtons;

namespace EasyConnect.Protocols.Vnc
{
    public partial class VncDesktop : UserControl, IRenderTarget
    {
        protected object _bitmapReplacementLock = new object();
        protected Bitmap _bitmap = null;
        protected FramebufferReference _currentFramebufferReference = null;

        protected HashSet<KeySymbol> _pressedKeys = new HashSet<KeySymbol>();

        /// <summary>Whether the remote framebuffer is scaled to the control bounds.</summary>
        public bool ScaleToFit { get; set; }

        public VncDesktop()
        {
            InitializeComponent();

            this.MouseWheel += VncDesktop_MouseWheel;
        }

        public void Reset()
        {
            if (_currentFramebufferReference != null)
            {
                _currentFramebufferReference.Dispose();
                _currentFramebufferReference = null;
            }

            if (_bitmap != null)
            {
                lock (_bitmapReplacementLock)
                {
                    _bitmap.Dispose();
                    _bitmap = null;
                }
            }
        }

        private void VncDesktop_MouseWheel(object sender, MouseEventArgs e)
        {
            HandlePointerEvent(e);
        }

        public VncConnection VncConnection
        {
            get;
            set;
        }

        public RfbConnection RfbConnection
        {
            get;
            set;
        }

        public IFramebufferReference GrabFramebufferReference(VncSize size, IImmutableSet<VncScreen> layout)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(VncDesktop));
            }

            bool sizeChanged = true;

            if (_bitmap != null)
            {
                lock (_bitmapReplacementLock)
                {
                    sizeChanged = _bitmap.Width != size.Width || _bitmap.Height != size.Height;
                }
            }

            if (sizeChanged)
            {
                lock (_bitmapReplacementLock)
                {
                    Bitmap newBitmap = new Bitmap(size.Width, size.Height, DrawingPixelFormat.Format32bppArgb);

                    if (_bitmap != null)
                    {
                        _bitmap.Dispose();
                    }

                    _bitmap = newBitmap;
                }
            }

            _currentFramebufferReference = new FramebufferReference(_bitmap, RenderFramebuffer);
            return _currentFramebufferReference;
        }

        private void VncDesktop_Paint(object sender, PaintEventArgs e)
        {
            if (_bitmap != null)
            {
                lock (_bitmapReplacementLock)
                {
                    if (_currentFramebufferReference == null)
                    {
                        if (ScaleToFit && ClientSize.Width > 0 && ClientSize.Height > 0)
                        {
                            float scale = Math.Min(
                                (float)ClientSize.Width / _bitmap.Width,
                                (float)ClientSize.Height / _bitmap.Height);
                            int width = Math.Max(1, (int)Math.Round(_bitmap.Width * scale));
                            int height = Math.Max(1, (int)Math.Round(_bitmap.Height * scale));
                            int left = (ClientSize.Width - width) / 2;
                            int top = (ClientSize.Height - height) / 2;
                            e.Graphics.Clear(BackColor);
                            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            e.Graphics.DrawImage(_bitmap, new System.Drawing.Rectangle(left, top, width, height));
                        }
                        else
                        {
                            e.Graphics.DrawImage(_bitmap, 0, 0);
                        }
                    }
                }
            }
        }

        protected void RenderFramebuffer()
        {
            if (_currentFramebufferReference != null)
            {
                _currentFramebufferReference = null;

                if (IsDisposed || Disposing || !IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine("Ignoring VNC framebuffer render callback while the desktop is closing.");
                    return;
                }

                try
                {
                    Invoke(new Action(() =>
                    {
                        if (!IsDisposed && !Disposing)
                        {
                            Invalidate();
                        }
                    }));
                }
                catch (ObjectDisposedException) when (IsDisposed || Disposing || !IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine("Ignoring VNC framebuffer render callback after the desktop was disposed.");
                }
                catch (InvalidOperationException) when (IsDisposed || Disposing || !IsHandleCreated)
                {
                    System.Diagnostics.Debug.WriteLine("Ignoring VNC framebuffer render callback while the desktop handle is unavailable.");
                }
            }
        }

        private void VncDesktop_MouseMove(object sender, MouseEventArgs e)
        {
            HandlePointerEvent(e);
        }

        private void VncDesktop_MouseDown(object sender, MouseEventArgs e)
        {
            HandlePointerEvent(e);
        }

        private void VncDesktop_MouseUp(object sender, MouseEventArgs e)
        {
            HandlePointerEvent(e);
        }

        private bool HandlePointerEvent(MouseEventArgs mouseData)
        {
            RfbConnection connection = RfbConnection;

            if (connection == null)
            {
                return false;
            }

            int remoteX = mouseData.X;
            int remoteY = mouseData.Y;

            // Convert client coordinates back to remote framebuffer coordinates
            // when the image is scaled to fit the available window.
            if (ScaleToFit && _bitmap != null && ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                float scale = Math.Min(
                    (float)ClientSize.Width / _bitmap.Width,
                    (float)ClientSize.Height / _bitmap.Height);
                int width = Math.Max(1, (int)Math.Round(_bitmap.Width * scale));
                int height = Math.Max(1, (int)Math.Round(_bitmap.Height * scale));
                int left = (ClientSize.Width - width) / 2;
                int top = (ClientSize.Height - height) / 2;

                remoteX = (int)((mouseData.X - left) / scale);
                remoteY = (int)((mouseData.Y - top) / scale);
                remoteX = Math.Max(0, Math.Min(_bitmap.Width - 1, remoteX));
                remoteY = Math.Max(0, Math.Min(_bitmap.Height - 1, remoteY));
            }

            Position position = new Position(remoteX, remoteY);
            VncMouseButtons buttonsMask = GetButtonsMask(mouseData.Button);
            VncMouseButtons wheelMask = GetWheelMask(mouseData.Delta);

            // For scrolling, set the wheel buttons and remove them quickly after that.
            if (wheelMask != VncMouseButtons.None)
            {
                connection.EnqueueMessage(new PointerEventMessage(position, buttonsMask | wheelMask));
            }

            connection.EnqueueMessage(new PointerEventMessage(position, buttonsMask));

            return true;
        }

        private VncMouseButtons GetWheelMask(int wheelDelta)
        {
            var mask = VncMouseButtons.None;

            if (wheelDelta > 0)
            {
                mask |= VncMouseButtons.WheelUp;
            }

            else if (wheelDelta < 0)
            {
                mask |= VncMouseButtons.WheelDown;
            }

            return mask;
        }

        private VncMouseButtons GetButtonsMask(FormsMouseButtons buttons)
        {
            var mask = VncMouseButtons.None;

            if ((buttons & FormsMouseButtons.Left) == FormsMouseButtons.Left)
            {
                mask |= VncMouseButtons.Left;
            }

            if ((buttons & FormsMouseButtons.Middle) == FormsMouseButtons.Middle)
            {
                mask |= VncMouseButtons.Middle;
            }

            if ((buttons & FormsMouseButtons.Right) == FormsMouseButtons.Right)
            {
                mask |= VncMouseButtons.Right;
            }

            return mask;
        }

        private void VncDesktop_MouseEnter(object sender, EventArgs e)
        {
            if (!VncConnection.ShowLocalCursor)
            {
                Cursor.Hide();
            }
        }

        private void VncDesktop_MouseLeave(object sender, EventArgs e)
        {
            if (!VncConnection.ShowLocalCursor)
            {
                Cursor.Show();
            }
        }

        private void VncDesktop_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.None)
            {
                return;
            }

            HandleKeyEvent(true, e.KeyCode, e.Modifiers);
        }

        private void VncDesktop_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.None)
            {
                return;
            }

            HandleKeyEvent(false, e.KeyCode, e.Modifiers);
        }

        private bool HandleKeyEvent(bool downFlag, Keys key, Keys keyModifiers)
        {
            // Get connection
            RfbConnection connection = RfbConnection;

            if (connection == null)
            {
                return false;
            }

            // Get key symbol
            KeySymbol keySymbol = VncKeyMapping.GetSymbolFromKey(key, keyModifiers);

            if (keySymbol == KeySymbol.Null)
            {
                return false;
            }

            // Send key event to server
            bool queued = connection.EnqueueMessage(new KeyEventMessage(downFlag, keySymbol));

            if (downFlag && queued)
            {
                _pressedKeys.Add(keySymbol);
            }

            else if (!downFlag)
            {
                _pressedKeys.Remove(keySymbol);
            }

            return queued;
        }

        private void VncDesktop_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Tab || e.KeyCode == Keys.Menu)
            {
                e.IsInputKey = true;
            }
        }
    }
}
