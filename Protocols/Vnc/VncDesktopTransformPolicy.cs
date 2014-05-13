using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using VncSharp;

namespace EasyConnect.Protocols.Vnc
{
	public abstract class VncDesktopTransformPolicy
	{
		protected VncClient vnc;
		protected VncDesktop remoteDesktop;

		public virtual bool AutoScroll
		{
			get
			{
				return false;
			}
		}

		public abstract Size AutoScrollMinSize { get; }

		public VncDesktopTransformPolicy(VncClient vnc, VncDesktop remoteDesktop)
		{
			this.vnc = vnc;
			this.remoteDesktop = remoteDesktop;
		}

		public abstract Rectangle AdjustUpdateRectangle(Rectangle updateRectangle);

		public abstract Rectangle RepositionImage(Image desktopImage);

		public abstract Rectangle GetMouseMoveRectangle();

		public abstract Point GetMouseMovePoint(Point current);

		public abstract Point UpdateRemotePointer(Point current);
	}
}
