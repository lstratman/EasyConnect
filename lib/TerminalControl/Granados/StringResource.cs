/* ---------------------------------------------------------------------------
 *
 * Copyright (c) Granados Networks, Inc.    All Rights Reserved..
 * 
 * This file is a part of the Granados SSH Client Library that is subject to
 * the license included in the distributed package.
 * You may not use this file except in compliance with the license.
 * 
 * $Id: StringResource.cs,v 1.2 2005/04/20 09:00:56 okajima Exp $
 */
using System;
using System.Globalization;
using System.Resources;
using System.Diagnostics;
using System.Reflection;

namespace Granados.SSHC {

	/// <summary>
	/// StringResource の概要の説明です。
	/// </summary>
	internal class StringResources {
		private string _resourceName;
		private ResourceManager _resMan;

		public StringResources(string name, Assembly asm) {
			_resourceName = name;
			LoadResourceManager(name, asm);
		}

		public string GetString(string id) {
			return _resMan.GetString(id); //もしこれが遅いようならこのクラスでキャッシュでもつくればいいだろう
		}

		private void LoadResourceManager(string name, Assembly asm) {
			//当面は英語・日本語しかしない
			CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentUICulture;
			if(ci.Name.StartsWith("ja"))
				_resMan = new ResourceManager(name+"_ja", asm);
			else
				_resMan = new ResourceManager(name, asm);
		}
	}
}