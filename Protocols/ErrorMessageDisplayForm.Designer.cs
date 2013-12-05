using System.Windows.Forms;

namespace EasyConnect.Protocols
{
	partial class ErrorMessageDisplayForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._okButton = new Button();
			this._errorMessageTextBox = new TextBox();
			this.SuspendLayout();
			this._okButton.Location = new System.Drawing.Point(285, 196);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 0;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			this._errorMessageTextBox.Location = new System.Drawing.Point(13, 13);
			this._errorMessageTextBox.Multiline = true;
			this._errorMessageTextBox.Name = "_errorMessageTextBox";
			this._errorMessageTextBox.ReadOnly = true;
			this._errorMessageTextBox.Size = new System.Drawing.Size(347, 173);
			this._errorMessageTextBox.TabIndex = 1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(372, 231);
			this.Controls.Add((Control)this._errorMessageTextBox);
			this.Controls.Add((Control)this._okButton);
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ErrorMessageDisplayForm";
			this.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Error message";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Button _okButton;
		private TextBox _errorMessageTextBox;
	}
}