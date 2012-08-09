/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: PeripheralOptionPanel.cs,v 1.2 2005/04/20 08:45:45 okajima Exp $
*/
using System;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Toolkit;
using Poderosa.Config;
using Poderosa.UI;

namespace Poderosa.Forms
{
	/// <summary>
	/// PeripheralOptionPanel の概要の説明です。
	/// </summary>
	internal class PeripheralOptionPanel : OptionDialog.CategoryPanel
	{
		private System.Windows.Forms.Label _leftAltKeyLabel;
		private ComboBox _leftAltKeyAction;
		private System.Windows.Forms.Label _rightAltKeyLabel;
		private ComboBox _rightAltKeyAction;
		private System.Windows.Forms.Label _rightButtonActionLabel;
		private ComboBox _rightButtonAction;
		private CheckBox _autoCopyByLeftButton;
		private Label _additionalWordElementLabel;
		private TextBox _additionalWordElementBox;
		private CheckBox _send0x7FByDel;
		private System.Windows.Forms.Label _wheelAmountLabel;
		private TextBox _wheelAmount;
		private Label _localBufferScrollModifierLabel;
		private ComboBox _localBufferScrollModifierBox;

		public PeripheralOptionPanel()
		{
			InitializeComponent();
			FillText();
		}
		private void InitializeComponent() {
			this._leftAltKeyLabel = new System.Windows.Forms.Label();
			this._leftAltKeyAction = new ComboBox();
			this._rightAltKeyLabel = new System.Windows.Forms.Label();
			this._rightAltKeyAction = new ComboBox();
			this._send0x7FByDel = new CheckBox();
			this._autoCopyByLeftButton = new CheckBox();
			this._rightButtonActionLabel = new Label();
			this._rightButtonAction = new ComboBox();
			this._wheelAmountLabel = new Label();
			this._wheelAmount = new TextBox();
			this._additionalWordElementLabel = new Label();
			this._additionalWordElementBox = new TextBox();
			this._localBufferScrollModifierLabel = new Label();
			this._localBufferScrollModifierBox = new ComboBox();

			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																								this._leftAltKeyLabel,
																								this._leftAltKeyAction,
																								this._rightAltKeyLabel,
																								this._rightAltKeyAction,
																								this._send0x7FByDel,
																								this._autoCopyByLeftButton,
																								this._additionalWordElementLabel,
																								this._additionalWordElementBox,
																								this._rightButtonActionLabel,
																								this._rightButtonAction,
																								this._wheelAmountLabel,
																								this._wheelAmount,
																								this._localBufferScrollModifierLabel,
																								this._localBufferScrollModifierBox});
			// 
			// _leftAltKeyLabel
			// 
			this._leftAltKeyLabel.Location = new System.Drawing.Point(24, 12);
			this._leftAltKeyLabel.Name = "_leftAltKeyLabel";
			this._leftAltKeyLabel.Size = new System.Drawing.Size(160, 23);
			this._leftAltKeyLabel.TabIndex = 0;
			this._leftAltKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _leftAltKey
			// 
			this._leftAltKeyAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._leftAltKeyAction.Location = new System.Drawing.Point(264, 12);
			this._leftAltKeyAction.Name = "_leftAltKey";
			this._leftAltKeyAction.Size = new System.Drawing.Size(152, 19);
			this._leftAltKeyAction.TabIndex = 1;
			// 
			// _rightAltKeyLabel
			// 
			this._rightAltKeyLabel.Location = new System.Drawing.Point(24, 36);
			this._rightAltKeyLabel.Name = "_rightAltKeyLabel";
			this._rightAltKeyLabel.Size = new System.Drawing.Size(160, 23);
			this._rightAltKeyLabel.TabIndex = 2;
			this._rightAltKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _rightAltKey
			// 
			this._rightAltKeyAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._rightAltKeyAction.Location = new System.Drawing.Point(264, 36);
			this._rightAltKeyAction.Name = "_rightAltKey";
			this._rightAltKeyAction.Size = new System.Drawing.Size(152, 19);
			this._rightAltKeyAction.TabIndex = 3;
			// 
			// _send0x7FByDel
			// 
			this._send0x7FByDel.Location = new System.Drawing.Point(24, 60);
			this._send0x7FByDel.Name = "_send0x7FByDel";
			this._send0x7FByDel.FlatStyle = FlatStyle.System;
			this._send0x7FByDel.Size = new System.Drawing.Size(192, 20);
			this._send0x7FByDel.TabIndex = 4;
			// 
			// _autoCopyByLeftButton
			// 
			this._autoCopyByLeftButton.Location = new System.Drawing.Point(24, 84);
			this._autoCopyByLeftButton.Name = "_autoCopyByLeftButton";
			this._autoCopyByLeftButton.FlatStyle = FlatStyle.System;
			this._autoCopyByLeftButton.Size = new System.Drawing.Size(288, 20);
			this._autoCopyByLeftButton.TabIndex = 5;
			// 
			// _additionalWordElementLabel
			// 
			this._additionalWordElementLabel.Location = new System.Drawing.Point(24, 108);
			this._additionalWordElementLabel.Size = new System.Drawing.Size(192, 23);
			this._additionalWordElementLabel.TabIndex = 6;
			this._additionalWordElementLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _additionalWordElementBox
			// 
			this._additionalWordElementBox.Location = new System.Drawing.Point(264, 108);
			this._additionalWordElementBox.Size = new System.Drawing.Size(152, 23);
			this._additionalWordElementBox.TabIndex = 7;
			// 
			// _rightButtonActionLabel
			// 
			this._rightButtonActionLabel.Location = new System.Drawing.Point(24, 132);
			this._rightButtonActionLabel.Name = "_rightButtonActionLabel";
			this._rightButtonActionLabel.Size = new System.Drawing.Size(160, 23);
			this._rightButtonActionLabel.TabIndex = 8;
			this._rightButtonActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _rightButtonAction
			// 
			this._rightButtonAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._rightButtonAction.Location = new System.Drawing.Point(264, 132);
			this._rightButtonAction.Name = "_rightButtonAction";
			this._rightButtonAction.Size = new System.Drawing.Size(152, 19);
			this._rightButtonAction.TabIndex = 9;
			// 
			// _wheelAmountLabel
			// 
			this._wheelAmountLabel.Location = new System.Drawing.Point(24, 156);
			this._wheelAmountLabel.Name = "_wheelAmountLabel";
			this._wheelAmountLabel.Size = new System.Drawing.Size(176, 23);
			this._wheelAmountLabel.TabIndex = 10;
			this._wheelAmountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _wheelAmount
			// 
			this._wheelAmount.Location = new System.Drawing.Point(264, 156);
			this._wheelAmount.Name = "_wheelAmount";
			this._wheelAmount.Size = new System.Drawing.Size(152, 19);
			this._wheelAmount.TabIndex = 11;
			this._wheelAmount.MaxLength = 2;
			// 
			// _localBufferScrollModifierLabel
			// 
			this._localBufferScrollModifierLabel.Location = new System.Drawing.Point(24, 180);
			this._localBufferScrollModifierLabel.Name = "_localBufferScrollModifierLabel";
			this._localBufferScrollModifierLabel.Size = new System.Drawing.Size(240, 23);
			this._localBufferScrollModifierLabel.TabIndex = 12;
			this._localBufferScrollModifierLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _localBufferScrollModifierBox
			// 
			this._localBufferScrollModifierBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._localBufferScrollModifierBox.Location = new System.Drawing.Point(264, 180);
			this._localBufferScrollModifierBox.Name = "_localBufferScrollModifierBox";
			this._localBufferScrollModifierBox.Size = new System.Drawing.Size(152, 19);
			this._localBufferScrollModifierBox.TabIndex = 13;
			
			this.BackColor = ThemeUtil.TabPaneBackColor;
		}
		private void FillText() {
			this._leftAltKeyLabel.Text = GApp.Strings.GetString("Form.OptionDialog._leftAltKeyLabel");
			this._rightAltKeyLabel.Text = GApp.Strings.GetString("Form.OptionDialog._rightAltKeyLabel");
			this._send0x7FByDel.Text = GApp.Strings.GetString("Form.OptionDialog._send0x7FByDel");
			this._autoCopyByLeftButton.Text = GApp.Strings.GetString("Form.OptionDialog._autoCopyByLeftButton");
			this._rightButtonActionLabel.Text = GApp.Strings.GetString("Form.OptionDialog._rightButtonActionLabel");
			this._wheelAmountLabel.Text = GApp.Strings.GetString("Form.OptionDialog._wheelAmountLabel");
			this._additionalWordElementLabel.Text = GApp.Strings.GetString("Form.OptionDialog._additionalWordElementLabel");
			this._localBufferScrollModifierLabel.Text = GApp.Strings.GetString("Form.OptionDialog._localBufferScrollModifierLabel");

			_leftAltKeyAction.Items.AddRange(EnumDescAttribute.For(typeof(AltKeyAction)).DescriptionCollection());
			_rightAltKeyAction.Items.AddRange(EnumDescAttribute.For(typeof(AltKeyAction)).DescriptionCollection());
			_rightButtonAction.Items.AddRange(EnumDescAttribute.For(typeof(RightButtonAction)).DescriptionCollection());
			_localBufferScrollModifierBox.Items.AddRange(new object[] { "Control", "Shift" });
		}
		public override void InitUI(ContainerOptions options) {
			_leftAltKeyAction.SelectedIndex = (int)options.LeftAltKey;
			_rightAltKeyAction.SelectedIndex = (int)options.RightAltKey;
			_send0x7FByDel.Checked = options.Send0x7FByDel;
			_autoCopyByLeftButton.Checked = options.AutoCopyByLeftButton;
			_rightButtonAction.SelectedIndex = (int)options.RightButtonAction;
			_wheelAmount.Text = options.WheelAmount.ToString();
			_additionalWordElementBox.Text = options.AdditionalWordElement;
			_localBufferScrollModifierBox.SelectedIndex = LocalBufferScrollModifierIndex(options.LocalBufferScrollModifier);
		}
		public override bool Commit(ContainerOptions options) {
			bool successful = false;
			string itemname = null;
			try {
				//Win9xでは、左右のAltの区別ができないので別々の設定にすることを禁止する
				if(System.Environment.OSVersion.Platform==PlatformID.Win32Windows &&
					_leftAltKeyAction.SelectedIndex!=_rightAltKeyAction.SelectedIndex) {
					GUtil.Warning(this, GApp.Strings.GetString("Message.OptionDialog.AltKeyOnWin9x"));
					return false;
				}

				options.LeftAltKey = (AltKeyAction)_leftAltKeyAction.SelectedIndex;
				options.RightAltKey = (AltKeyAction)_rightAltKeyAction.SelectedIndex;
				options.Send0x7FByDel = _send0x7FByDel.Checked;
				options.AutoCopyByLeftButton = _autoCopyByLeftButton.Checked;
				options.RightButtonAction = (RightButtonAction)_rightButtonAction.SelectedIndex;

				itemname = GApp.Strings.GetString("Caption.OptionDialog.MousewheelAmount");
				options.WheelAmount = Int32.Parse(_wheelAmount.Text);
				options.LocalBufferScrollModifier = LocalBufferScrollModifierKey(_localBufferScrollModifierBox.SelectedIndex);

				foreach(char ch in _additionalWordElementBox.Text) {
					if(ch>=0x100) {
						GUtil.Warning(this, GApp.Strings.GetString("Message.OptionDialog.InvalidAdditionalWordElement"));
						return false;
					}
				}
				options.AdditionalWordElement = _additionalWordElementBox.Text;
						
				successful = true;
			}
			catch(FormatException) {
				GUtil.Warning(this, String.Format(GApp.Strings.GetString("Message.OptionDialog.InvalidItem"), itemname));
			}
			catch(InvalidOptionException ex) {
				GUtil.Warning(this, ex.Message);
			}
			return successful;
		}

		private static int LocalBufferScrollModifierIndex(Keys key) {
			if(key==Keys.Control)
				return 0;
			else if(key==Keys.Shift)
				return 1;
			else if(key==Keys.Alt)
				return 2;
			else
				return -1; //never comes
		}
		private static Keys LocalBufferScrollModifierKey(int index) {
			if(index==0)
				return Keys.Control;
			else if(index==1)
				return Keys.Shift;
			else if(index==2)
				return Keys.Alt;
			else
				return Keys.Control;
		}
	}
}
