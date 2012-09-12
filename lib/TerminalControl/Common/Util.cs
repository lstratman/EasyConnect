/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.

 $Id: Util.cs,v 1.2 2005/04/20 09:06:03 okajima Exp $
*/
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace Poderosa.UI
{
	internal class Win32 {
		[DllImport("gdi32.dll")]
		public static extern uint SetPixel(IntPtr hDC, int x, int y, uint colorref);
		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
		[DllImport("gdi32.dll")]
		public static extern int DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll")]
		public static extern int MoveToEx(IntPtr hDC, int x, int y, IntPtr prev_point); //‘O‚ÌˆÊ’u‚É‚Í‹»–¡‚È‚µ
		public static int MoveToEx(IntPtr hDC, int x, int y) {
			return MoveToEx(hDC, x, y, IntPtr.Zero/*NULL*/);
		}
		[DllImport("gdi32.dll")]
		public static extern int LineTo(IntPtr hDC, int x, int y);
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreatePen(int style, int width, uint color);
	}

	public class UILibUtil {
		public static Thread CreateThread(ThreadStart st) {
			Thread t = new Thread(st);
			//t.ApartmentState = ApartmentState.STA;
            t.SetApartmentState(ApartmentState.STA);
			return t;
		}

		public static string KeyString(Keys key) {
			int ik = (int)key;
			if((int)Keys.D0<=ik && ik<=(int)Keys.D9)
				return new string((char)('0' + (ik-(int)Keys.D0)), 1);
			else {
				switch(key) {
					case Keys.None:
						return "";
					case Keys.Prior:
						return "PageUp";
					case Keys.Next:
						return "PageDown";
						//Oem‚Ù‚É‚á‚ç‚ç‚ª‚¤‚´‚Á‚½‚¢
					case Keys.OemBackslash:
						return "Backslash";
					case Keys.OemCloseBrackets:
						return "CloseBrackets";
					case Keys.Oemcomma:
						return "Comma";
					case Keys.OemMinus:
						return "Minus";
					case Keys.OemOpenBrackets:
						return "OpenBrackets";
					case Keys.OemPeriod:
						return "Period";
					case Keys.OemPipe:
						return "Pipe";
					case Keys.Oemplus:
						return "Plus";
					case Keys.OemQuestion:
						return "Question";
					case Keys.OemQuotes:
						return "Quotes";
					case Keys.OemSemicolon:
						return "Semicolon";
					case Keys.Oemtilde:
						return "Tilde";
					default:
						return key.ToString();
				}
			}
		}

		public static string KeyString(Keys modifiers, Keys body, char delimiter) {
			StringBuilder b = new StringBuilder();
			if((modifiers & Keys.Control)!=Keys.None) {
				b.Append("Ctrl");
			}
			if((modifiers & Keys.Shift)!=Keys.None) {
				if(b.Length>0) b.Append(delimiter);
				b.Append("Shift");
			}
			if((modifiers & Keys.Alt)!=Keys.None) {
				if(b.Length>0) b.Append(delimiter);
				b.Append("Alt");
			}
			if(b.Length>0)
				b.Append(delimiter);

			b.Append(KeyString(body));
			return b.ToString();
		}
	}
}
