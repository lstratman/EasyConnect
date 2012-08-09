/*
 * Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 * $Id: ColorUtil.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
 */
using System;
using System.Drawing;

namespace Poderosa.UI
{
	public class ColorUtil {

		static public Color VSNetBackgroundColor {
			get {
				return CalculateColor(SystemColors.Window, SystemColors.Control, 220);
			}
		}

		static public Color VSNetSelectionColor {
			get {
				return CalculateColor(SystemColors.Highlight, SystemColors.Window, 70);
			}
		}


		static public Color VSNetControlColor {
			get {
				return CalculateColor(SystemColors.Control, VSNetBackgroundColor, 195);
			}

		}

		static public Color VSNetPressedColor {
			get {
				return CalculateColor(SystemColors.Highlight, VSNetSelectionColor, 70);
			}
		}


		static public Color VSNetCheckedColor {
			get {
				return CalculateColor(SystemColors.Highlight,  SystemColors.Window, 30);
			}
		}

		public static Color CalculateColor(Color front, Color back, int alpha) {
			// Use alpha blending to brigthen the colors but don't use it
			// directly. Instead derive an opaque color that we can use.
			// -- if we use a color with alpha blending directly we won't be able 
			// to paint over whatever color was in the background and there
			// would be shadows of that color showing through
			Color frontColor = Color.FromArgb(255, front);
			Color backColor = Color.FromArgb(255, back);
									
			float frontRed = frontColor.R;
			float frontGreen = frontColor.G;
			float frontBlue = frontColor.B;
			float backRed = backColor.R;
			float backGreen = backColor.G;
			float backBlue = backColor.B;
			
			float fRed = frontRed*alpha/255 + backRed*((float)(255-alpha)/255);
			byte newRed = (byte)fRed;
			float fGreen = frontGreen*alpha/255 + backGreen*((float)(255-alpha)/255);
			byte newGreen = (byte)fGreen;
			float fBlue = frontBlue*alpha/255 + backBlue*((float)(255-alpha)/255);
			byte newBlue = (byte)fBlue;

			return  Color.FromArgb(255, newRed, newGreen, newBlue);
		}
	}

}
