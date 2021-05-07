using MarcusW.VncClient.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VncSize = MarcusW.VncClient.Size;
using VncScreen = MarcusW.VncClient.Screen;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace EasyConnect.Protocols.Vnc
{
    public partial class VncDesktop : UserControl, IRenderTarget
    {
        protected Bitmap _bitmap = null;

        public VncDesktop()
        {
            InitializeComponent();
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
                sizeChanged = _bitmap.Width != size.Width || _bitmap.Height != size.Height;
            }

            if (sizeChanged)
            {
                Bitmap newBitmap = new Bitmap(size.Width, size.Height, DrawingPixelFormat.Format32bppArgb);

                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                }

                _bitmap = newBitmap;
            }

            return new FramebufferReference(_bitmap, RenderFramebuffer);
        }

        private void VncDesktop_Paint(object sender, PaintEventArgs e)
        {
            if (_bitmap != null)
            {
                e.Graphics.DrawImage(_bitmap, 0, 0);
            }
        }

        protected void RenderFramebuffer()
        {
            Invoke(new Action(() =>
            {
                Invalidate();
            }));
        }
    }
}
