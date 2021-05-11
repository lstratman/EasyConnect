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
                        e.Graphics.DrawImage(_bitmap, 0, 0);
                    }
                }
            }
        }

        protected void RenderFramebuffer()
        {
            if (_currentFramebufferReference != null)
            {
                _currentFramebufferReference = null;

                Invoke(new Action(() =>
                {
                    Invalidate();
                }));
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

            Position position = new Position(mouseData.X, mouseData.Y);
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
