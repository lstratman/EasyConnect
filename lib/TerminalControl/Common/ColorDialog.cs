/*
 * Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 * 
 * this source code originates in Johannes Wallroth's color picker control.
 * http://www.codeproject.com/cs/miscctrl/color_picker.asp
 * 
 * $Id: ColorDialog.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace Poderosa.UI
{
	internal class ColorPaletteDialog : Form
	{
		private bool _isClosing;

		byte max = 40;	
		Panel[] panel = new Panel[40];	
		
		Color[] color = new Color[40]
		{
			//row 1
			Color.FromArgb(0,0,0), Color.FromArgb(153,51,0), Color.FromArgb(51,51,0), Color.FromArgb(0,51,0),
			Color.FromArgb(0,51,102), Color.FromArgb(0,0,128), Color.FromArgb(51,51,153), Color.FromArgb(51,51,51),
			
			//row 2
			Color.FromArgb(128,0,0), Color.FromArgb(255,102,0), Color.FromArgb(128,128,0), Color.FromArgb(0,128,0),
			Color.FromArgb(0,128,128), Color.FromArgb(0,0,255), Color.FromArgb(102,102,153), Color.FromArgb(128,128,128),
			
			//row 3
			Color.FromArgb(255,0,0), Color.FromArgb(255,153,0), Color.FromArgb(153,204,0), Color.FromArgb(51,153,102),
			Color.FromArgb(51,204,204), Color.FromArgb(51,102,255), Color.FromArgb(128,0,128), Color.FromArgb(153,153,153),
			
			//row 4
			Color.FromArgb(255,0,255), Color.FromArgb(255,204,0), Color.FromArgb(255,255,0), Color.FromArgb(0,255,0),
			Color.FromArgb(0,255,255), Color.FromArgb(0,204,255), Color.FromArgb(153,51,102), Color.FromArgb(192,192,192),
			
			//row 5
			Color.FromArgb(255,153,204), Color.FromArgb(255,204,153), Color.FromArgb(255,255,153), Color.FromArgb(204,255,204),
			Color.FromArgb(204,255,255), Color.FromArgb(153,204,255), Color.FromArgb(204,153,255), Color.FromArgb(255,255,255)						
		};	
		
		string[] colorName = new string[40]
		{
			"Black", "Brown", "Olive Green", "Dark Green", "Dark Teal", "Dark Blue", "Indigo", "Gray-80%",
			"Dark Red", "Orange", "Dark Yellow", "Green", "Teal", "Blue", "Blue-Gray", "Gray-50%",
			"Red", "Light Orange", "Lime", "Sea Green", "Aqua", "Light Blue", "Violet", "Gray-40%",
			"Pink", "Gold", "Yellow", "Bright Green", "Turquoise", "Sky Blue", "Plum", "Gray-25%",
			"Rose", "Tan", "Light Yellow", "Light Green", "Light Turquoise", "Pale Blue", "Lavender", "White"
		};	
		
		Button moreColorsButton = new Button();
		Button cancelButton = new Button();
		Color selectedColor;
		
		public ColorPaletteDialog(int x, int y)
		{
			Size = new Size(158, 132);					
			FormBorderStyle = FormBorderStyle.FixedDialog;	
			MinimizeBox = MaximizeBox = ControlBox = false;				
			ShowInTaskbar = false;					
			CenterToScreen();	
			Location = new Point(x, y);	
			
			BuildPalette();						

			moreColorsButton.Text = "Others...";			
			moreColorsButton.Size = new Size(142, 22);
			moreColorsButton.Location = new Point(5, 99);		
			moreColorsButton.Click += new EventHandler(moreColorsButton_Click);		
			moreColorsButton.FlatStyle = FlatStyle.Popup;
			Controls.Add(moreColorsButton);	
			
			//"invisible" button to cancel at Escape
			cancelButton.Size = new Size(5, 5);
			cancelButton.Location = new Point(-10, -10);		
			cancelButton.Click += new EventHandler(cancelButton_Click);				
			Controls.Add(cancelButton);	
			cancelButton.TabIndex = 0;
			cancelButton.DialogResult = DialogResult.Cancel;
			this.CancelButton = cancelButton;
		}
		
		public Color Color
		{
			get {return selectedColor;}
		}
		
		void BuildPalette()
		{
			byte pwidth = 16;
			byte pheight = 16;
			byte pdistance = 2;		
			byte border = 5;		
			int x = border, y = border;	
			ToolTip toolTip = new ToolTip();		
			
			for(int i = 0; i < max; i++)
			{
				panel[i] = new Panel();			
				panel[i].Height = pwidth;
				panel[i].Width = pheight;			
				panel[i].Location = new Point(x, y);
				toolTip.SetToolTip(panel[i], colorName[i]);
						
				this.Controls.Add(panel[i]);			
				
				if(x < ( 7 * (pwidth + pdistance)))
					x += pwidth + pdistance;
				else
				{
					x = border;
					y += pheight + pdistance;
				}	
					
				panel[i].BackColor = color[i];
				panel[i].MouseEnter += new EventHandler(OnMouseEnterPanel);
				panel[i].MouseLeave += new EventHandler(OnMouseLeavePanel);			
				panel[i].MouseDown += new MouseEventHandler(OnMouseDownPanel);			
				panel[i].MouseUp += new MouseEventHandler(OnMouseUpPanel);						
				panel[i].Paint += new PaintEventHandler(OnPanelPaint);						
			}
		}
		
		void moreColorsButton_Click(object sender, System.EventArgs e)
		{    
    		ColorDialog colDialog = new ColorDialog();
    		colDialog.FullOpen = true;
			this.DialogResult = colDialog.ShowDialog();
			if(this.DialogResult == DialogResult.OK)    	
    			selectedColor = colDialog.Color;
    		colDialog.Dispose();

			_isClosing = true;
			Close();
		}
	    
		void cancelButton_Click(object sender, System.EventArgs e)
		{        	
    		Close();
		}
			
		void OnMouseEnterPanel(object sender, EventArgs e)
		{			
			DrawPanel(sender, 1);
		}
		
		void OnMouseLeavePanel(object sender, EventArgs e)
		{			
			DrawPanel(sender, 0);	
		}	
		
		void OnMouseDownPanel(object sender, MouseEventArgs e)
		{				
			DrawPanel(sender, 2);				
		}
		
		void OnMouseUpPanel(object sender, MouseEventArgs e)
		{			
			Panel panel = (Panel)sender;		
			selectedColor = panel.BackColor;
			DialogResult = DialogResult.OK;
			_isClosing = true;
			Close();
		}
		
		void DrawPanel(object sender, byte state)
		{		
			Panel panel = (Panel)sender;			
			
			Graphics g = panel.CreateGraphics();
			
			Pen pen1, pen2;
			
			if(state == 1) 		//mouse over
			{
				pen1 = new Pen( SystemColors.ControlLightLight ); 				
				pen2 = new Pen( SystemColors. ControlDarkDark);		
			}
			else if(state == 2)	//clicked
			{
				pen1 = new Pen( SystemColors.ControlDarkDark ); 				
				pen2 = new Pen( SystemColors.ControlLightLight );						
			}
			else				//neutral
			{
				pen1 = new Pen( SystemColors.ControlDark ); 				
				pen2 = new Pen( SystemColors.ControlDark );
				
			}	
			
			Rectangle r = panel.ClientRectangle;
			Point p1 = new Point( r.Left, r.Top ); 				//top left
			Point p2 = new Point( r.Right -1, r.Top );			//top right
			Point p3 = new Point( r.Left, r.Bottom -1 );		//bottom left
			Point p4 = new Point( r.Right -1, r.Bottom -1 );	//bottom right
			
			g.DrawLine( pen1, p1, p2 ); 		
			g.DrawLine( pen1, p1, p3 ); 		
			g.DrawLine( pen2, p2, p4 ); 		
			g.DrawLine( pen2, p3, p4 ); 				
		}
		
		void OnPanelPaint(Object sender, PaintEventArgs e)
		{					
			DrawPanel(sender, 0);		
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			if(m.Msg == 0x0086/*WM_NCACTIVATE*/ && m.WParam.ToInt32() == 0 && !_isClosing) {
				this.DialogResult = DialogResult.Cancel;
				Close();
			}
		}


	}

}

