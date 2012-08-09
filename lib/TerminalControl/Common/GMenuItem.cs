/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GMenuItem.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*
*  this source code originats in OfficeMenu component by Mohammed Halabi
*/

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Text;

namespace Poderosa.UI
{
	public abstract class GMenuItemBase : MenuItem {
		protected int _id;
		protected Keys _shortcut;
	
		public GMenuItemBase() {
			_shortcut = Keys.None;
			base.OwnerDraw = true;
		}
		public GMenuItemBase(GMenuItemBase mi) {
			CloneMenu(mi);
			_shortcut = Keys.None;
			base.OwnerDraw = true;
		}

		public Keys ShortcutKey {
			get {
				return _shortcut;
			}
			set {
				_shortcut = value;
			}
		}
		public int CID {
			get {
				return _id;
			}
			set {
				_id = value;
			}
		}

		protected class Consts {
			public static int PIC_AREA_SIZE = 24;
			public static int MIN_MENU_HEIGHT = 22;
			public static Font menuFont = System.Windows.Forms.SystemInformation.MenuFont; 
			public static Color CheckBoxColor = Color.FromArgb(255, 192, 111);
			public static Color DarkCheckBoxColor = Color.FromArgb(254, 128, 62);
			public static Color SelectionColor = Color.FromArgb(255,238,194);
			public static Color TextColor = Color.FromKnownColor(KnownColor.MenuText);
			public static Color TextDisabledColor = Color.FromKnownColor(KnownColor.GrayText);
			public static Color MenuBgColor = Color.White;
			public static Color MainColor = Color.FromKnownColor(KnownColor.Control);
			public static Color MenuDarkColor = Color.FromKnownColor(KnownColor.ActiveCaption);
			public static Color MenuLightColor = Color.FromKnownColor(KnownColor.ControlDark); //InactiveCaption
			public static Color MenuDarkColor2 = Color.FromArgb(160, Color.FromKnownColor(KnownColor.ActiveCaption));
			public static Color MenuLightColor2 = Color.FromArgb(40, Color.FromKnownColor(KnownColor.InactiveCaption));

		}
	}

	public class GMainMenuItem : GMenuItemBase {
		
		private static SolidBrush _mainBrush = new SolidBrush(Consts.MainColor);
		private static Pen        _mainPen   = new Pen(Consts.MainColor);
		private static SolidBrush _textBrush = new SolidBrush(Consts.TextColor);

		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			string miText = this.Text.Replace("&","");
			SizeF miSize = e.Graphics.MeasureString(miText, Consts.menuFont);
			e.ItemWidth = Convert.ToInt32(miSize.Width);
		}
		protected override void OnDrawItem(DrawItemEventArgs e) {
			base.OnDrawItem(e);
			// Draw the main menu item
			DrawMenuItem(e);
		}

		public void DrawMenuItem(DrawItemEventArgs e) {

			// Check the state of the menuItem
			if ( (e.State & DrawItemState.HotLight) == DrawItemState.HotLight ) {
				// Draw Hover rectangle
				DrawHoverRect(e);
			} 
			else if ( (e.State & DrawItemState.Selected) == DrawItemState.Selected ) {
				// Draw selection rectangle
				DrawSelectionRect(e);
			} else {
				// if no selection, just draw space
				Rectangle rect = new Rectangle(e.Bounds.X, 
					e.Bounds.Y, 
					e.Bounds.Width, 
					e.Bounds.Height -1);

				e.Graphics.FillRectangle(_mainBrush, rect);
				e.Graphics.DrawRectangle(_mainPen, rect);
			}
			
			// Create stringFormat object
			StringFormat sf = new StringFormat();

			// set the Alignment to center
			sf.LineAlignment = StringAlignment.Center;
			sf.Alignment = StringAlignment.Center;
			sf.HotkeyPrefix = HotkeyPrefix.Show;

			// Draw the text
			e.Graphics.DrawString(this.Text, 
				Consts.menuFont, 
				_textBrush, 
				e.Bounds, 
				sf);	
		}

		private void DrawHoverRect(DrawItemEventArgs e) {
			// Create the hover rectangle
			Rectangle rect = new Rectangle(e.Bounds.X, 
				e.Bounds.Y + 1, 
				e.Bounds.Width, 
				e.Bounds.Height - 2);

			// Create the hover brush
			Brush b = new LinearGradientBrush(rect, 
				Color.White, 
				Consts.CheckBoxColor,
				90f, false);

			// Fill the rectangle
			e.Graphics.FillRectangle(b, rect);

			// Draw borders
			e.Graphics.DrawRectangle(new Pen(Color.Black), rect);
		}

		private void DrawSelectionRect(DrawItemEventArgs e) {
			// Create the selection rectangle
			Rectangle rect = new Rectangle(e.Bounds.X, 
				e.Bounds.Y + 1, 
				e.Bounds.Width, 
				e.Bounds.Height - 2);

			// Create the selectino brush
			Brush b = new LinearGradientBrush(rect, 
				Consts.MenuBgColor, 
				Consts.MenuDarkColor2,
				90f, false);

			// fill the rectangle
			e.Graphics.FillRectangle(b, rect);

			// Draw borders
			e.Graphics.DrawRectangle(new Pen(Color.Black), rect);
		}
	}

	public class GMenuItem : GMenuItemBase {
		protected static SolidBrush _menuBgBrush = new SolidBrush(Consts.MenuBgColor);
		protected static SolidBrush _textBrush   = new SolidBrush(Consts.TextColor);
		protected static SolidBrush _textDisabledBrush = new SolidBrush(Consts.TextDisabledColor);	
		protected static SolidBrush _selectionBrush = new SolidBrush(Consts.SelectionColor);
		protected static Pen _menuDarkPen = new Pen(Consts.MenuDarkColor);
		protected static Pen _menuLightPen = new Pen(Consts.MenuLightColor);

		public GMenuItem() {
		}
		public GMenuItem(GMenuItem mi) : base(mi) {
		}

		public override MenuItem CloneMenu() {
			GMenuItem mi = new GMenuItem(this);
			mi._shortcut = _shortcut;
			mi.Visible = this.Visible;
			mi.Enabled = this.Enabled;
			mi.CID = this.CID;
			return mi;
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			// if the item is a seperator
			if ( this.Text == "-" ) {
				e.ItemHeight = 7;
				e.ItemWidth = 16;
				Debug.Assert(this.OwnerDraw);
			} else {
				string miText = this.Text.Replace("&","");
				// get the item's text size
				SizeF miSize = e.Graphics.MeasureString(miText, Consts.menuFont);
				int scWidth = 0;
				// get the short cut width
				if (_shortcut != Keys.None ) {
					SizeF scSize = e.Graphics.MeasureString(FormatShortcut(_shortcut), Consts.menuFont);
					scWidth = Convert.ToInt32(scSize.Width);
				}
				// set the bounds
				int miHeight = Convert.ToInt32(miSize.Height) + 7;
				if (miHeight < 25) miHeight = Consts.MIN_MENU_HEIGHT;
				e.ItemHeight = miHeight;
				e.ItemWidth = Convert.ToInt32(miSize.Width) + scWidth + (Consts.PIC_AREA_SIZE * 2);
			}
		}

		protected override void OnDrawItem(DrawItemEventArgs e) {
			// Draw Menu Item
			DrawMenuItem(e);
		}
		public void DrawMenuItem(DrawItemEventArgs e) {
		
			// check to see if the menu item is selected
			if ( (e.State & DrawItemState.Selected) == DrawItemState.Selected ) {
				// Draw selection rectangle
				DrawSelectionRect(e);	
			} else {
				// if no selection, just draw white space
				e.Graphics.FillRectangle(_menuBgBrush, e.Bounds);
				// Draw the picture area
				DrawPictureArea(e);
			}

			// Draw check box if the menu item is checked
			if ( (e.State & DrawItemState.Checked) == DrawItemState.Checked ) {
				DrawCheckBox(e);
			}

			// Draw the menuitem text
			DrawMenuText(e);

			// Draw the item's picture
			DrawItemPicture(e);

		}

		protected virtual void DrawMenuText(DrawItemEventArgs e) {
			Brush textBrush = _textBrush;

			// Draw the menu text
			// if the menu item is a seperator
			if ( this.Text == "-" ) {
				// draw seperator line
				e.Graphics.DrawLine(_menuLightPen, e.Bounds.X + Consts.PIC_AREA_SIZE + 6, e.Bounds.Y + 2, e.Bounds.Width, e.Bounds.Y + 2);
			} else {
				// create StringFormat object and set the alignment to center
				StringFormat sf = new StringFormat();
				sf.LineAlignment = StringAlignment.Center;
				sf.HotkeyPrefix = HotkeyPrefix.Show;

				// create the rectangle that will hold the text
				RectangleF rect = new Rectangle(Consts.PIC_AREA_SIZE + 4, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

				// check the menuitem status
				if ( this.Enabled )
					textBrush = _textBrush;	
				else
					textBrush = _textDisabledBrush;
				
				// Draw the text
				e.Graphics.DrawString(this.Text, Consts.menuFont, textBrush, rect, sf);

				// Draw the shortcut text
				DrawShortCutText(e);
			}
		}

		private void DrawShortCutText(DrawItemEventArgs e) {
			// check to see if there is a short cut for this item
			if ( this.ShortcutKey != Keys.None) {
				string shortcut_text = FormatShortcut(this.ShortcutKey);
				// get the shortcut text size
				SizeF scSize = 
					e.Graphics.MeasureString(shortcut_text, 
					Consts.menuFont);

				// Create the text rectangle
				Rectangle rect = 
					new Rectangle(e.Bounds.Width - Convert.ToInt32(scSize.Width) - 3,
					e.Bounds.Y,
					Convert.ToInt32(scSize.Width) + 3,
					e.Bounds.Height);

				// set it to right-to-left, and center it
				StringFormat sf = new StringFormat();
				//sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
				sf.LineAlignment = StringAlignment.Center;

				// draw the text
				if ( this.Enabled )
					e.Graphics.DrawString(shortcut_text, 
						Consts.menuFont, 
						_textBrush, 
						rect, sf);
				else {	
					// if menuItem is disabled
					e.Graphics.DrawString(shortcut_text, 
						Consts.menuFont, 
						_textDisabledBrush, 
						rect, sf);
				}
			}
		}

		private void DrawPictureArea(DrawItemEventArgs e) {
			// the picture area rectangle
			Rectangle rect = new Rectangle(e.Bounds.X - 1, 
				e.Bounds.Y, 
				Consts.PIC_AREA_SIZE, 
				e.Bounds.Height);

			// Create Gradient brush, using system colors
			Brush b = new LinearGradientBrush(rect, 
				Consts.MenuDarkColor2, 
				Consts.MenuLightColor2,
				180f, 
				false);

			// Draw the rect
			e.Graphics.FillRectangle(b, rect);
			b.Dispose();
		}

		private void DrawItemPicture(DrawItemEventArgs e) {
			const int MAX_PIC_SIZE = 16;

			// Get the Item's picture
			Image img = null; //OfficeMenus.GetItemPicture(mi);

			// check to see if the Item has a picture, if none, Ignore
			if ( img != null ) {
				// if the size exceeds the maximum picture's size, fix it
				int width = img.Width > MAX_PIC_SIZE ? MAX_PIC_SIZE : img.Width;
				int height = img.Height > MAX_PIC_SIZE ? MAX_PIC_SIZE : img.Height;
				
				// set the picture coordinates
				int x = e.Bounds.X + 2;
				int y = e.Bounds.Y + ((e.Bounds.Height - height) / 2);
				
				// create the picture rectangle
				Rectangle rect = new Rectangle(x, y, width, height);
				
				// Now check the items state, if enabled just draw the picture
				// if not enabled, make a water mark and draw it.
				if ( this.Enabled ) {
					// draw the image
					e.Graphics.DrawImage(img, x, y, width, height);
				} else {
					// make water mark of the picture
					ColorMatrix myColorMatrix = new ColorMatrix();
					myColorMatrix.Matrix00 = 1.00f; // Red
					myColorMatrix.Matrix11 = 1.00f; // Green
					myColorMatrix.Matrix22 = 1.00f; // Blue
					myColorMatrix.Matrix33 = 1.30f; // alpha
					myColorMatrix.Matrix44 = 1.00f; // w

					// Create an ImageAttributes object and set the color matrix.
					ImageAttributes imageAttr = new ImageAttributes();
					imageAttr.SetColorMatrix(myColorMatrix);

					// draw the image
					e.Graphics.DrawImage(img,
						rect,
						0, 
						0, 
						width, 
						height, 
						GraphicsUnit.Pixel, 
						imageAttr);
				}
			}
		}

		private void DrawSelectionRect(DrawItemEventArgs e) {
			// if the item is not enabled, then do not draw the selection rect
			if ( this.Enabled ) {
				// fill selection rectangle
				e.Graphics.FillRectangle(_selectionBrush, 
					e.Bounds);

				// Draw borders
				e.Graphics.DrawRectangle(_menuDarkPen, 
					e.Bounds.X, 
					e.Bounds.Y, 
					e.Bounds.Width - 1, 
					e.Bounds.Height - 1);
			}
		}

		private void DrawCheckBox(DrawItemEventArgs e) {
			// Define the CheckBox size
			int cbSize = Consts.PIC_AREA_SIZE - 5;

			// set the smoothing mode to anti alias
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			// the main rectangle
			Rectangle rect = new Rectangle(e.Bounds.X + 1, 
				e.Bounds.Y + ((e.Bounds.Height - cbSize) / 2), 
				cbSize, 
				cbSize);

			// construct the drawing pen
			Pen pen = new Pen(Color.Black,1.7f);

			// fill the rectangle
			if ( (e.State & DrawItemState.Selected) == DrawItemState.Selected )
				e.Graphics.FillRectangle(new SolidBrush(Consts.DarkCheckBoxColor), rect);
			else
				e.Graphics.FillRectangle(new SolidBrush(Consts.CheckBoxColor), rect);

			// draw borders
			e.Graphics.DrawRectangle(_menuDarkPen, rect);
			
			// Check to see if the menuItem has a picture
			// if Yes, Do not draw the check mark; else, Draw it
			Bitmap img = null; //OfficeMenus.GetItemPicture(mi);

			if ( img == null ) {
				// Draw the check mark
				e.Graphics.DrawLine(pen, e.Bounds.X + 7, 
					e.Bounds.Y + 10, 
					e.Bounds.X + 10, 
					e.Bounds.Y + 14);

				e.Graphics.DrawLine(pen, 
					e.Bounds.X + 10, 
					e.Bounds.Y + 14, 
					e.Bounds.X + 15, 
					e.Bounds.Y + 9);
			}
		}

		public static string FormatShortcut(Keys key) {
			Keys modifiers = key & Keys.Modifiers;
			StringBuilder b = new StringBuilder();
			if((modifiers & Keys.Control)!=Keys.None) {
				b.Append("Ctrl");
			}
			if((modifiers & Keys.Shift)!=Keys.None) {
				if(b.Length>0) b.Append('+');
				b.Append("Shift");
			}
			if((modifiers & Keys.Alt)!=Keys.None) {
				if(b.Length>0) b.Append('+');
				b.Append("Alt");
			}
			if(b.Length>0)
				b.Append('+');

			b.Append(UILibUtil.KeyString(key & Keys.KeyCode));
			return b.ToString();
		}
	}

}
