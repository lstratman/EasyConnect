/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GIcons.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Resources;

namespace Poderosa
{
	internal class GIcons {
		private static ResourceManager _resMan;

		public static Icon GetBellIcon() {
			if(_resMan==null) LoadResource();
			return (Icon)_resMan.GetObject("bell_icon");
		}

		public static Icon GetAppIcon() {
			if(_resMan==null) LoadResource();
			return (Icon)_resMan.GetObject("app_icon");
		}
		public static Icon GetOldGuevaraIcon() {
			if(_resMan==null) LoadResource();
			return (Icon)_resMan.GetObject("old_icon");
		}

		private static void LoadResource() {
			_resMan = new ResourceManager("Poderosa.icons", typeof(GIcons).Assembly);
		}
	}
}
