/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: GDocument.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Text;
using Poderosa.Toolkit;
using Poderosa.Forms;

namespace Poderosa.Text
{
	/// <summary>
	/// ドキュメントのベースクラス。ここから派生してログ用とターミナル用のドキュメントがある。
	/// GLineのコレクションを保有する。
	/// </summary>
	internal abstract class GDocument {

		protected GDocument() {
		}

		public abstract void AddLine(GLine line);

		/// <summary>
		/// 行数取得
		/// </summary>
		public abstract int Size {
			get;
		}

		public abstract void Dump(string title);
	}

}
