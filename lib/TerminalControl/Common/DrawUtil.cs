/*
 * Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 * $Id: DrawUtil.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
 */
using System;
using System.Drawing;
using System.Diagnostics;

namespace Poderosa.UI {
	public class DrawUtil {
		//丸みのあるBorderを描画する
		internal enum RoundBorderElement {
			Outer,
			Inner,
			Light,
			LightLight
		}
		public class RoundRectColors {
			public uint border_color;
			public uint outer_color;
			public uint inner_color;
			public uint light_color;
			public uint lightlight_color;

			internal uint GetColor(RoundBorderElement e) {
				switch(e) {
					case RoundBorderElement.Inner:
						return inner_color;
					case RoundBorderElement.Outer:
						return outer_color;
					case RoundBorderElement.Light:
						return light_color;
					case RoundBorderElement.LightLight:
						return lightlight_color;
				}
				Debug.Assert(false, "should not reach here");
				return 0;
			}
							  
		}
		
		//左上の丸みを描くとき、[x,y]で参照する
		private static readonly RoundBorderElement[,] _round_border_info = new RoundBorderElement[3,3] {
          { RoundBorderElement.Outer,      RoundBorderElement.LightLight, RoundBorderElement.Light      },
          {	RoundBorderElement.LightLight, RoundBorderElement.Light,      RoundBorderElement.LightLight },
          { RoundBorderElement.Light,      RoundBorderElement.LightLight, RoundBorderElement.Inner      },
		};

		public static void DrawRoundRect(Graphics g, int x, int y, int width, int height, RoundRectColors colors) {
			IntPtr hdc = g.GetHdc();
			const int ROUND_SIZE = 3; //3*3ピクセルは自前で描画
			IntPtr pen = Win32.CreatePen(0, 1, colors.border_color);
			Win32.SelectObject(hdc, pen);
			//上
			Win32.MoveToEx(hdc, x+ROUND_SIZE, y);
			Win32.LineTo(hdc, x+width-ROUND_SIZE+1, y);
			//下
			Win32.MoveToEx(hdc, x+ROUND_SIZE, y+height);
			Win32.LineTo(hdc, x+width-ROUND_SIZE+1, y+height);
			//左
			Win32.MoveToEx(hdc, x, y+ROUND_SIZE);
			Win32.LineTo(hdc, x, y+height-ROUND_SIZE+1);
			//右
			Win32.MoveToEx(hdc, x+width, y+ROUND_SIZE);
			Win32.LineTo(hdc, x+width, y+height-ROUND_SIZE+1);
			
			Win32.DeleteObject(pen);
			
			DrawRoundCorner(hdc, x,       y,        1, 1, colors); //左上
			DrawRoundCorner(hdc, x+width, y,       -1, 1, colors); //右上
			DrawRoundCorner(hdc, x,       y+height, 1,-1, colors); //左下
			DrawRoundCorner(hdc, x+width, y+height,-1,-1, colors); //右下

			g.ReleaseHdc(hdc);
		}

		//配列の参照に回転がかかっているのに注意
		private static void DrawRoundCorner(IntPtr hdc, int bx, int by, int dx, int dy, RoundRectColors colors) {
			int y = by;
			for(int j=0; j<3; j++) {
				int x = bx;
				for(int i=0; i<3; i++) {
					Win32.SetPixel(hdc, x, y, colors.GetColor(_round_border_info[i,j]));
					x += dx;
				}
				y += dy;
			}
		}
			
		//輝度を半分にした色を返す
		public static Color DarkColor(Color src) {
			return Color.FromArgb(src.R/2, src.G/2, src.B/2);
		}
		public static Color LightColor(Color src) {
			return Color.FromArgb(src.R/2+128, src.G/2+128, src.B/2+128);
		}

		//COLORREFに対応した整数を返す
		public static uint ToCOLORREF(Color c) {
			uint t = (uint)c.ToArgb();
			//COLORREFは0x00BBGGRR、ToArgbは0x00RRGGBB
			uint r = (t & 0x00FF0000) >> 16;
			uint b = (t & 0x000000FF) << 16;
			t &= 0x0000FF00;
			return t | r | b;
		}
		public static uint MergeColor(uint col1, uint col2) {
			uint r = (((col1 & 0x0000FF) + (col2 & 0x0000FF)) >> 1) & 0x0000FF;
			uint g = (((col1 & 0x00FF00) + (col2 & 0x00FF00)) >> 1) & 0x00FF00;
			uint b = (((col1 & 0xFF0000) + (col2 & 0xFF0000)) >> 1) & 0xFF0000;
			return r | g | b;
		}
	}
}
