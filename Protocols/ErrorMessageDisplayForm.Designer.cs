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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorMessageDisplayForm));
			this._okButton = new System.Windows.Forms.Button();
			this._errorMessageTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// _okButton
			// 
			this._okButton.Location = new System.Drawing.Point(285, 196);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 0;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			// 
			// _errorMessageTextBox
			// 
			this._errorMessageTextBox.Location = new System.Drawing.Point(13, 13);
			this._errorMessageTextBox.Multiline = true;
			this._errorMessageTextBox.Name = "_errorMessageTextBox";
			this._errorMessageTextBox.ReadOnly = true;
			this._errorMessageTextBox.Size = new System.Drawing.Size(347, 173);
			this._errorMessageTextBox.TabIndex = 1;
			// 
			// ErrorMessageDisplayForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(372, 231);
			this.Controls.Add(this._errorMessageTextBox);
			this.Controls.Add(this._okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ErrorMessageDisplayForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Error message";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Button _okButton;
		private TextBox _errorMessageTextBox;
	}
}