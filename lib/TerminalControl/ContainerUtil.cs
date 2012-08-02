/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: ContainerUtil.cs,v 1.2 2005/04/20 08:45:44 okajima Exp $
*/
using System;
using System.Globalization;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Poderosa.Forms;
using Poderosa.Config;
using Poderosa.UI;

namespace Poderosa
{
	internal enum LogFileCheckResult {
		Create,
		Append,
		Cancel,
		Error
	}

	internal class GCUtil : GUtil
	{
		//既存のファイルであったり、書き込み不可能だったら警告する
		public static LogFileCheckResult CheckLogFileName(string path, Form parent) {
			try {
				if(path.Length==0) {
					GUtil.Warning(parent, GApp.Strings.GetString("Message.CheckLogFileName.EmptyPath"));
					return LogFileCheckResult.Cancel;
				}

				if(File.Exists(path)) {
					if((FileAttributes.ReadOnly & File.GetAttributes(path)) != (FileAttributes)0) {
						GUtil.Warning(parent, String.Format(GApp.Strings.GetString("Message.CheckLogFileName.NotWritable"), path));
						return LogFileCheckResult.Cancel;
					}
				
					Poderosa.Forms.ThreeButtonMessageBox mb = new Poderosa.Forms.ThreeButtonMessageBox();
					mb.Message = String.Format(GApp.Strings.GetString("Message.CheckLogFileName.AlreadyExist"), path);
					mb.Text = GApp.Strings.GetString("Util.CheckLogFileName.Caption");
					mb.YesButtonText = GApp.Strings.GetString("Util.CheckLogFileName.OverWrite");
					mb.NoButtonText  = GApp.Strings.GetString("Util.CheckLogFileName.Append");
					mb.CancelButtonText = GApp.Strings.GetString("Util.CheckLogFileName.Cancel");
					switch(GCUtil.ShowModalDialog(parent, mb)) {
						case DialogResult.Cancel:
							return LogFileCheckResult.Cancel;
						case DialogResult.Yes: //上書き
							return LogFileCheckResult.Create;
						case DialogResult.No:  //追記
							return LogFileCheckResult.Append;
						default:
							break;
					}
				}

				return LogFileCheckResult.Create; //!!書き込み可能なディレクトリにあることを確認すればなおよし

			}
			catch(Exception ex) {
				GUtil.Warning(parent, ex.Message);
				return LogFileCheckResult.Error;
			}
		}

		//ダイアログでログファイルを開く
		public static string SelectLogFileByDialog(Form parent) {
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.AddExtension = true;
			dlg.DefaultExt = "log";
			dlg.Title = GApp.Strings.GetString("Util.SelectLogFileByDialog.Caption");
			dlg.Filter = "Log Files(*.log)|*.log|All Files(*.*)|*.*";
			if(GCUtil.ShowModalDialog(parent, dlg)==DialogResult.OK) {
				return dlg.FileName;
			}
			else
				return null;
		}
		public static string SelectPrivateKeyFileByDialog(Form parent) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.CheckFileExists = true;
			dlg.Multiselect = false;
			dlg.InitialDirectory = GApp.Options.DefaultKeyDir;
			dlg.Title = GApp.Strings.GetString("Util.SelectPrivateKey.Caption");
			dlg.Filter = "Key Files(*.bin;*)|*.bin;*";
			if(GCUtil.ShowModalDialog(parent, dlg)==DialogResult.OK) {
				GApp.Options.DefaultKeyDir = FileToDir(dlg.FileName);
				return dlg.FileName;
			}
			else
				return null;
		}
		public static string SelectPictureFileByDialog(Form parent) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.CheckFileExists = true;
			dlg.Multiselect = false;
			dlg.InitialDirectory = GApp.Options.DefaultFileDir;
			dlg.Title = GApp.Strings.GetString("Util.SelectPicture.Caption");
			dlg.Filter = "Picture Files(*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
			if(GCUtil.ShowModalDialog(parent, dlg)==DialogResult.OK) {
				GApp.Options.DefaultFileDir = FileToDir(dlg.FileName);
				return dlg.FileName;
			}
			else
				return null;
		}

		//.NET1.1SP1 対策で、ダイアログ表示の手続きにひとくせあり
		public static DialogResult ShowModalDialog(Form parent, Form dialog) {
			DialogResult r;
#if false
			r = dialog.ShowDialog(parent);
#else
			if(parent.Modal) {
				if(GApp.Options.HideDialogForSP1Issue) {
					parent.Visible = false;
					r = dialog.ShowDialog(parent);
					parent.Visible = true;
				}
				else {
					parent.Enabled = false;
					r = dialog.ShowDialog(parent);
					parent.Enabled = true;
				}
			}
			else {
				parent.Enabled = false;
				r = dialog.ShowDialog(parent);
				parent.Enabled = true;
			}
#endif
			dialog.Dispose();
			return r;
		}
		//ShowDialogを使わずにそれっぽく見せる
		public static void ShowPseudoModalDialog(Form parent, Form dialog) {
			dialog.Owner = GApp.Frame;
			//centering
			dialog.Left = parent.Left + parent.Width/2  - dialog.Width/2;
			dialog.Top  = parent.Top  + parent.Height/2 - dialog.Height/2;

			parent.Enabled = false;
			dialog.Show();
		}
		public static DialogResult ShowModalDialog(Form parent, CommonDialog dialog) {
			parent.Enabled = false;
			DialogResult r = dialog.ShowDialog();
			parent.Enabled = true;
			dialog.Dispose();
			return r;
		}

	}

}
