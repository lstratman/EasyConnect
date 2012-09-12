/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: DebugUtil.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.Text;

namespace Poderosa.Toolkit
{
	internal class DebugUtil
	{
		public static string DumpByteArray(byte[] data) {
			return DumpByteArray(data, 0, data.Length);
		}
		public static string DumpByteArray(byte[] data, int offset, int length) {
			StringBuilder bld = new StringBuilder();
			for(int i=0; i<length; i++) {
				bld.Append(data[offset+i].ToString("X2"));
				if((i % 4)==3) bld.Append(' ');
			}
			return bld.ToString();
		}
	}
}
