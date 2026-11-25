using MarcusW.VncClient;
using MarcusW.VncClient.Protocol;
using MarcusW.VncClient.Protocol.EncodingTypes;
using MarcusW.VncClient.Protocol.Implementation;
using MarcusW.VncClient.Protocol.Implementation.EncodingTypes.Frame;
using MarcusW.VncClient.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConnect.Protocols.Vnc
{
    public class VncProtocolImplementation : DefaultImplementation
    {
        public string PreferredEncodingType
        {
            get;
            set;
        }

        internal bool UseAmtZrleDecoder { get; set; }

        public override IImmutableSet<IEncodingType> CreateEncodingTypesCollection(RfbConnectionContext context)
        {
            if (!UseAmtZrleDecoder && String.IsNullOrEmpty(PreferredEncodingType))
            {
                return base.CreateEncodingTypesCollection(context);
            }

            List<IEncodingType> encodingTypes = GetDefaultEncodingTypes(context).ToList();
            if (UseAmtZrleDecoder)
            {
                int index = encodingTypes.FindIndex(type => type is ZrleEncodingType);
                if (index < 0)
                    throw new InvalidOperationException("The VNC protocol does not provide a ZRLE decoder.");
                encodingTypes[index] = new AmtZrleEncodingType(context);
            }
            IEncodingType preferredEncoding = encodingTypes.SingleOrDefault(t => t.Name == PreferredEncodingType);

            if (preferredEncoding != null)
            {
                encodingTypes.Remove(preferredEncoding);
                encodingTypes.Insert(0, preferredEncoding);
            }

            return encodingTypes.ToImmutableHashSet();
        }
    }

    internal sealed class AmtZrleEncodingType : ZrleEncodingType
    {
        private readonly RfbConnectionContext _context;

        internal AmtZrleEncodingType(RfbConnectionContext context) : base(context)
        {
            _context = context;
        }

        public override void ReadFrameEncoding(Stream transportStream, IFramebufferReference targetFramebuffer,
            in Rectangle rectangle, in Size remoteFramebufferSize, in PixelFormat remoteFramebufferFormat)
        {
            if (remoteFramebufferFormat.BitsPerPixel != 16 || !remoteFramebufferFormat.TrueColor || remoteFramebufferFormat.HasAlpha)
            {
                base.ReadFrameEncoding(transportStream, targetFramebuffer, rectangle, remoteFramebufferSize, remoteFramebufferFormat);
                return;
            }

            if (transportStream == null)
                throw new ArgumentNullException(nameof(transportStream));
            if (rectangle.IsEmpty() || !rectangle.FitsInside(remoteFramebufferSize))
                throw new RfbProtocolException("Invalid ZRLE rectangle bounds.");
            if (remoteFramebufferFormat.RedMax == 0 || remoteFramebufferFormat.GreenMax == 0 || remoteFramebufferFormat.BlueMax == 0)
                throw new RfbProtocolException("Invalid ZRLE pixel format.");

            uint compressedLength = 0;
            for (int i = 0; i < 4; i++)
                compressedLength = (compressedLength << 8) | (uint)ReadByte(transportStream);
            if (compressedLength > Int32.MaxValue)
                throw new RfbProtocolException("ZRLE compressed data is too large.");

            Stream data = _context.ZLibInflater.ReadAndInflate(transportStream, (int)compressedLength);
            int right = rectangle.Position.X + rectangle.Size.Width;
            int bottom = rectangle.Position.Y + rectangle.Size.Height;
            for (int y = rectangle.Position.Y; y < bottom; y += 64)
            {
                for (int x = rectangle.Position.X; x < right; x += 64)
                {
                    var tile = new Rectangle(x, y, Math.Min(64, right - x), Math.Min(64, bottom - y));
                    DecodeTile(data, targetFramebuffer, tile, remoteFramebufferFormat);
                }
            }
        }

        internal static void DecodeTile(Stream data, IFramebufferReference framebuffer, Rectangle tile, PixelFormat format)
        {
            int encoding = ReadByte(data);
            int paletteSize = encoding & 127;
            bool rle = (encoding & 128) != 0;
            if ((!rle && paletteSize > 16) || (rle && paletteSize == 1))
                throw new RfbProtocolException("Invalid ZRLE tile encoding.");

            var palette = new Color[paletteSize];
            for (int i = 0; i < paletteSize; i++)
                palette[i] = ReadColor(data, format);

            FramebufferCursor cursor = framebuffer == null ? default(FramebufferCursor) : new FramebufferCursor(framebuffer, tile);
            int remaining = tile.Size.Width * tile.Size.Height;
            if (!rle && paletteSize == 1)
            {
                if (framebuffer != null)
                    cursor.SetPixelsSolid(palette[0], remaining);
                return;
            }

            if (!rle && paletteSize >= 2)
            {
                int bits = paletteSize <= 2 ? 1 : paletteSize <= 4 ? 2 : 4;
                int mask = (1 << bits) - 1;
                for (int y = 0; y < tile.Size.Height; y++)
                {
                    int packed = 0;
                    int available = 0;
                    for (int x = 0; x < tile.Size.Width; x++)
                    {
                        if (available == 0)
                        {
                            packed = ReadByte(data);
                            available = 8;
                        }
                        available -= bits;
                        int index = (packed >> available) & mask;
                        if (index >= paletteSize)
                            throw new RfbProtocolException("Invalid ZRLE palette index.");
                        if (framebuffer != null)
                            cursor.SetPixelsSolid(palette[index], 1);
                    }
                }
                return;
            }

            while (remaining > 0)
            {
                Color color;
                int run = 1;
                if (!rle || paletteSize == 0)
                {
                    color = ReadColor(data, format);
                    if (rle)
                        run = ReadRunLength(data, remaining);
                }
                else
                {
                    int index = ReadByte(data);
                    if ((index & 127) >= paletteSize)
                        throw new RfbProtocolException("Invalid ZRLE palette index.");
                    color = palette[index & 127];
                    if ((index & 128) != 0)
                        run = ReadRunLength(data, remaining);
                }

                if (framebuffer != null)
                    cursor.SetPixelsSolid(color, run);
                remaining -= run;
            }
        }

        private static int ReadRunLength(Stream data, int remaining)
        {
            int run = 1;
            int next;
            do
            {
                next = ReadByte(data);
                if (next > remaining - run)
                    throw new RfbProtocolException("ZRLE run exceeds its tile.");
                run += next;
            }
            while (next == 255);
            return run;
        }

        private static Color ReadColor(Stream data, PixelFormat format)
        {
            int first = ReadByte(data);
            int second = ReadByte(data);
            int pixel = format.BigEndian ? (first << 8) | second : first | (second << 8);
            return new Color(
                (byte)(((pixel >> format.RedShift) & format.RedMax) * 255 / format.RedMax),
                (byte)(((pixel >> format.GreenShift) & format.GreenMax) * 255 / format.GreenMax),
                (byte)(((pixel >> format.BlueShift) & format.BlueMax) * 255 / format.BlueMax));
        }

        private static int ReadByte(Stream data)
        {
            int value = data.ReadByte();
            if (value < 0)
                throw new UnexpectedEndOfStreamException("Stream ended while reading ZRLE tile data.");
            return value;
        }
    }
}
