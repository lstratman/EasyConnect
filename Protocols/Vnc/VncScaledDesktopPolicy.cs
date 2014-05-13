using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using VncSharp;

namespace EasyConnect.Protocols.Vnc
{
	public sealed class VncScaledDesktopPolicy : VncDesktopTransformPolicy
	{
		public VncScaledDesktopPolicy(VncClient vnc, VncDesktop remoteDesktop)
			: base(vnc, remoteDesktop)
		{
		}

		public override Size AutoScrollMinSize
		{
			get
			{
				return new Size(100, 100);
			}
		}

		public override Rectangle AdjustUpdateRectangle(Rectangle updateRectangle)
		{
			Size scaledSize = GetScaledSize(remoteDesktop.ClientRectangle.Size);
			Rectangle adjusted = new Rectangle(AdjusteNormalToScaled(updateRectangle.X) + ((remoteDesktop.ClientRectangle.Width - scaledSize.Width) / 2),
											   AdjusteNormalToScaled(updateRectangle.Y) + ((remoteDesktop.ClientRectangle.Height - scaledSize.Height) / 2),
											   AdjusteNormalToScaled(updateRectangle.Width),
											   AdjusteNormalToScaled(updateRectangle.Height));
			adjusted.Inflate(1, 1);
			return adjusted;
		}

		public override Rectangle RepositionImage(Image desktopImage)
		{
			return GetScaledRectangle(remoteDesktop.ClientRectangle);
		}

		public override Point UpdateRemotePointer(Point current)
		{
			return GetScaledMouse(current);
		}

		public override Rectangle GetMouseMoveRectangle()
		{
			return GetScaledRectangle(remoteDesktop.ClientRectangle);
		}

		public override Point GetMouseMovePoint(Point current)
		{
			return GetScaledMouse(current);
		}

		private Size GetScaledSize(Size s)
		{
			if (vnc == null)
				return new Size(remoteDesktop.Width, remoteDesktop.Height);

			if (((double)s.Width / vnc.Framebuffer.Width) <= ((double)s.Height / vnc.Framebuffer.Height))
			{
				return new Size(s.Width, (int)((double)s.Width / vnc.Framebuffer.Width * vnc.Framebuffer.Height));
			}
			else
			{
				return new Size((int)((double)s.Height / vnc.Framebuffer.Height * vnc.Framebuffer.Width), s.Height);
			}
		}

		private double ScaleFactor
		{
			get
			{
				if (((double)remoteDesktop.ClientRectangle.Width / vnc.Framebuffer.Width) <=
					((double)remoteDesktop.ClientRectangle.Height / vnc.Framebuffer.Height))
				{
					return ((double)remoteDesktop.ClientRectangle.Width / vnc.Framebuffer.Width);
				}
				else
				{
					return ((double)remoteDesktop.ClientRectangle.Height / vnc.Framebuffer.Height);
				}
			}
		}

		private Point GetScaledMouse(Point src)
		{
			Size scaledSize = GetScaledSize(remoteDesktop.ClientRectangle.Size);
			src.X = AdjusteScaledToNormal(src.X - ((remoteDesktop.ClientRectangle.Width - scaledSize.Width) / 2));
			src.Y = AdjusteScaledToNormal(src.Y - ((remoteDesktop.ClientRectangle.Height - scaledSize.Height) / 2));
			return src;
		}

		private Rectangle GetScaledRectangle(Rectangle rect)
		{
			Size scaledSize = GetScaledSize(rect.Size);
			return new Rectangle((rect.Width - scaledSize.Width) / 2,
								 (rect.Height - scaledSize.Height) / 2,
								 scaledSize.Width,
								 scaledSize.Height);
		}

		private int AdjusteScaledToNormal(double value)
		{
			return (int)Math.Round(value / ScaleFactor);
		}

		private int AdjusteNormalToScaled(double value)
		{
			return (int)Math.Round(value * ScaleFactor);
		}
	}
}
