/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: IconList.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Poderosa
{
	/// <summary>
	/// IconList の概要の説明です。
	/// </summary>
	internal class IconList : System.Windows.Forms.Form
	{
		System.Windows.Forms.ImageList _imageList;
		private System.ComponentModel.IContainer components;

		public IconList()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows フォーム デザイナで生成されたコード 
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(IconList));
			this._imageList = new System.Windows.Forms.ImageList(this.components);
			// 
			// _imageList
			// 
			this._imageList.ImageSize = new System.Drawing.Size(16, 16);
			this._imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imageList.ImageStream")));
			this._imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// IconList
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Name = "IconList";
			this.Text = "IconList";

		}
		#endregion

		private static IconList _iconList;

		public const int ICON_NEWCONNECTION = 0;
		public const int ICON_SERIAL = 1;
		public const int ICON_CYGWIN = 2;
		public const int ICON_SFU = 3;
		public const int ICON_OPEN = 4;
		public const int ICON_SAVE = 5;
		public const int ICON_SINGLE = 6;
		public const int ICON_DIVHORIZONTAL = 7;
		public const int ICON_DIVVERTICAL = 8;
		public const int ICON_DIVHORIZONTAL3 = 9;
		public const int ICON_DIVVERTICAL3 = 10;
		public const int ICON_LOCALECHO = 11;
		public const int ICON_LINEFEED = 12;
		public const int ICON_SUSPENDLOG = 13;
		public const int ICON_COMMENTLOG = 14;
		public const int ICON_INFO = 15;
		public const int ICON_BELL = 16;
		
		public static Image LoadIcon(int id) {
			if(_iconList==null)
				_iconList = new IconList();
			return _iconList._imageList.Images[id];
		}

	}
}
