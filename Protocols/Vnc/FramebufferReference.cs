using MarcusW.VncClient.Rendering;
using System;
using FramebufferSize = MarcusW.VncClient.Size;
using FramebufferPixelFormat = MarcusW.VncClient.PixelFormat;
using System.Drawing;
using System.Drawing.Imaging;

namespace EasyConnect.Protocols.Vnc
{
    public class FramebufferReference : IFramebufferReference
    {
        private Action _renderCallback;
        private Bitmap _bitmap;
        private BitmapData _bitmapLockData;

        private static FramebufferPixelFormat ARGBPixelFormat = new FramebufferPixelFormat("ARGB", 32, 32, false, true, true, 255, 255, 255, 255, 16, 8, 0, 24);
        private static float _horizontalDpi;
        private static float _verticalDpi;

        static FramebufferReference()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                _horizontalDpi = graphics.DpiX;
                _verticalDpi = graphics.DpiY;
            }
        }

        public FramebufferReference(Bitmap bitmap, Action renderCallback)
        {
            Format = ARGBPixelFormat;
            Size = new FramebufferSize(bitmap.Width, bitmap.Height);

            _bitmap = bitmap;
            _renderCallback = renderCallback;
            _bitmapLockData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        }

        public IntPtr Address
        {
            get
            {
                return _bitmapLockData.Scan0;
            }
        }

        public FramebufferSize Size
        {
            get;
            private set;
        }

        public FramebufferPixelFormat Format
        {
            get;
            private set;
        }

        public double HorizontalDpi
        {
            get
            {
                return _horizontalDpi;
            }
        }

        public double VerticalDpi
        {
            get
            {
                return _verticalDpi;
            }
        }

        public void Dispose()
        {
            _bitmap.UnlockBits(_bitmapLockData);
            _renderCallback();
        }
    }
}
