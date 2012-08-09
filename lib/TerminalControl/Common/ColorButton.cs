/*
 * Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 * $Id: ColorButton.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace Poderosa.UI
{
	public class ColorButton : Button {			
		private Color _selectedColor;

		public delegate void NewColorEventHandler(object sender, Color newcolor);
		public event NewColorEventHandler ColorChanged;
		
		public ColorButton() {
			this.BackColor = SystemColors.Control;
			this.FlatStyle = FlatStyle.Standard;
			this.SetStyle(ControlStyles.UserPaint, true);
		}
		
		public Color SelectedColor {
			get {
				return _selectedColor;
			}
			set {
				_selectedColor = value;
			}
		}		

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			Graphics g = e.Graphics;
			Rectangle r = this.ClientRectangle;
			
			const int border = 3;
			const int right_border = 15;
			
			Rectangle rc = new Rectangle(border, border,
										r.Width - border - right_border - 1, r.Height - border * 2 - 1);
		
			SolidBrush centerColorBrush = new SolidBrush( this.Enabled? _selectedColor : this.BackColor);
			g.FillRectangle(centerColorBrush, rc);	
			g.DrawRectangle( SystemPens.ControlDarkDark, rc );
			
			Pen pen = new Pen( this.Enabled? Color.Black : SystemColors.ControlDark ); 

			//draw the arrow
			Point p1 = new Point( r.Width - 9, r.Height / 2 - 1 );
			Point p2 = new Point(r.Width - 5, r.Height / 2 - 1 );		
			g.DrawLine(pen, p1, p2);
			
			p1 = new Point( r.Width - 8, r.Height / 2 );
			p2 = new Point(r.Width - 6, r.Height / 2 );		
			g.DrawLine(pen, p1, p2);
			
			p1 = new Point( r.Width - 7, r.Height / 2 );
			p2 = new Point(r.Width - 7, r.Height / 2 + 1 );		
			g.DrawLine(pen, p1, p2);
			pen.Dispose();
			
			//draw the divider line
			pen = SystemPens.ControlDark; 
			p1 = new Point( r.Width - 12, 4 );
			p2 = new Point(r.Width - 12, r.Height - 5 );		
			g.DrawLine(pen, p1, p2);
			
			pen = SystemPens.ControlLightLight;
			p1 = new Point( r.Width - 11, 4 );
			p2 = new Point(r.Width - 11, r.Height - 5 );		
			g.DrawLine(pen, p1, p2);

			centerColorBrush.Dispose();
		}
	    
		protected override void OnClick(EventArgs e) {
			Point p = PointToScreen(new Point(0, this.Height));
			ColorPaletteDialog clDlg = new ColorPaletteDialog(p.X, p.Y);
			clDlg.ShowDialog(FindForm());
			if(clDlg.DialogResult == DialogResult.OK) {
				_selectedColor = clDlg.Color;
				if(this.ColorChanged != null) this.ColorChanged(this, clDlg.Color);
			}
			Invalidate();
			clDlg.Dispose();
		}
	}

}
