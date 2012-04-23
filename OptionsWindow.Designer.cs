using SecurePasswordTextBox;

namespace EasyConnect
{
    partial class OptionsWindow
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
			System.Security.SecureString secureString1 = new System.Security.SecureString();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsWindow));
			this._sidebarContainer = new System.Windows.Forms.Panel();
			this._rdpDefaultsLabel = new System.Windows.Forms.Label();
			this._optionsLabel = new System.Windows.Forms.Label();
			this._topBorderPanel = new System.Windows.Forms.Panel();
			this._userNameTextBox = new System.Windows.Forms.TextBox();
			this._userNameLabel = new System.Windows.Forms.Label();
			this._selectedOptionCategoryTitle = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this._passwordLabel = new System.Windows.Forms.Label();
			this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
			this._generalLabel = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this._displayLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._resolutionLabel = new System.Windows.Forms.Label();
			this._resolutionSlider = new System.Windows.Forms.TrackBar();
			this._colorDepthDropdown = new System.Windows.Forms.ComboBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this._localResourcesLabel = new System.Windows.Forms.Label();
			this._remoteAudioRecordingLabel = new System.Windows.Forms.Label();
			this._remoteAudioPlaybackLabel = new System.Windows.Forms.Label();
			this._audioPlaybackDropdown = new System.Windows.Forms.ComboBox();
			this._audioRecordingDropdown = new System.Windows.Forms.ComboBox();
			this._windowsKeyDropdown = new System.Windows.Forms.ComboBox();
			this._windowsKeysLabel = new System.Windows.Forms.Label();
			this._localDevicesLabel = new System.Windows.Forms.Label();
			this._printersCheckbox = new System.Windows.Forms.CheckBox();
			this._clipboardCheckbox = new System.Windows.Forms.CheckBox();
			this._drivesCheckbox = new System.Windows.Forms.CheckBox();
			this._desktopCompositionCheckbox = new System.Windows.Forms.CheckBox();
			this._fontSmoothingCheckbox = new System.Windows.Forms.CheckBox();
			this._desktopBackgroundCheckbox = new System.Windows.Forms.CheckBox();
			this._allowTheFollowingLabel = new System.Windows.Forms.Label();
			this._experienceLabel = new System.Windows.Forms.Label();
			this._windowContentsWhileDraggingCheckbox = new System.Windows.Forms.CheckBox();
			this._menuAnimationCheckbox = new System.Windows.Forms.CheckBox();
			this._visualStylesCheckbox = new System.Windows.Forms.CheckBox();
			this._bitmapCachingCheckbox = new System.Windows.Forms.CheckBox();
			this._resolutionSliderLabel = new System.Windows.Forms.Label();
			this._sidebarContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._resolutionSlider)).BeginInit();
			this.SuspendLayout();
			// 
			// _sidebarContainer
			// 
			this._sidebarContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this._sidebarContainer.BackgroundImage = global::EasyConnect.Properties.Resources.OptionsSidebarBackground;
			this._sidebarContainer.Controls.Add(this._rdpDefaultsLabel);
			this._sidebarContainer.Controls.Add(this._optionsLabel);
			this._sidebarContainer.Location = new System.Drawing.Point(0, 0);
			this._sidebarContainer.Margin = new System.Windows.Forms.Padding(0);
			this._sidebarContainer.Name = "_sidebarContainer";
			this._sidebarContainer.Size = new System.Drawing.Size(217, 682);
			this._sidebarContainer.TabIndex = 0;
			// 
			// _rdpDefaultsLabel
			// 
			this._rdpDefaultsLabel.BackColor = System.Drawing.Color.Transparent;
			this._rdpDefaultsLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this._rdpDefaultsLabel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._rdpDefaultsLabel.Image = global::EasyConnect.Properties.Resources.SelectedOptionCategoryBackground;
			this._rdpDefaultsLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._rdpDefaultsLabel.Location = new System.Drawing.Point(-1, 55);
			this._rdpDefaultsLabel.Margin = new System.Windows.Forms.Padding(0);
			this._rdpDefaultsLabel.Name = "_rdpDefaultsLabel";
			this._rdpDefaultsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 17, 0);
			this._rdpDefaultsLabel.Size = new System.Drawing.Size(217, 33);
			this._rdpDefaultsLabel.TabIndex = 1;
			this._rdpDefaultsLabel.Text = "RDP Defaults";
			this._rdpDefaultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _optionsLabel
			// 
			this._optionsLabel.BackColor = System.Drawing.Color.Transparent;
			this._optionsLabel.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._optionsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(99)))), ((int)(((byte)(125)))));
			this._optionsLabel.Location = new System.Drawing.Point(5, 16);
			this._optionsLabel.Name = "_optionsLabel";
			this._optionsLabel.Size = new System.Drawing.Size(196, 31);
			this._optionsLabel.TabIndex = 0;
			this._optionsLabel.Text = "Options";
			this._optionsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// _topBorderPanel
			// 
			this._topBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._topBorderPanel.BackColor = System.Drawing.Color.Silver;
			this._topBorderPanel.Location = new System.Drawing.Point(0, 0);
			this._topBorderPanel.Name = "_topBorderPanel";
			this._topBorderPanel.Size = new System.Drawing.Size(740, 1);
			this._topBorderPanel.TabIndex = 1;
			// 
			// _userNameTextBox
			// 
			this._userNameTextBox.Location = new System.Drawing.Point(478, 65);
			this._userNameTextBox.Name = "_userNameTextBox";
			this._userNameTextBox.Size = new System.Drawing.Size(154, 20);
			this._userNameTextBox.TabIndex = 2;
			// 
			// _userNameLabel
			// 
			this._userNameLabel.Location = new System.Drawing.Point(322, 64);
			this._userNameLabel.Name = "_userNameLabel";
			this._userNameLabel.Size = new System.Drawing.Size(150, 20);
			this._userNameLabel.TabIndex = 3;
			this._userNameLabel.Text = "User name:";
			this._userNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _selectedOptionCategoryTitle
			// 
			this._selectedOptionCategoryTitle.BackColor = System.Drawing.Color.Transparent;
			this._selectedOptionCategoryTitle.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._selectedOptionCategoryTitle.ForeColor = System.Drawing.Color.Black;
			this._selectedOptionCategoryTitle.Location = new System.Drawing.Point(237, 16);
			this._selectedOptionCategoryTitle.Name = "_selectedOptionCategoryTitle";
			this._selectedOptionCategoryTitle.Size = new System.Drawing.Size(196, 31);
			this._selectedOptionCategoryTitle.TabIndex = 4;
			this._selectedOptionCategoryTitle.Text = "RDP Defaults";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.Color.Silver;
			this.panel1.Location = new System.Drawing.Point(242, 51);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(471, 1);
			this.panel1.TabIndex = 5;
			// 
			// _passwordLabel
			// 
			this._passwordLabel.Location = new System.Drawing.Point(322, 90);
			this._passwordLabel.Name = "_passwordLabel";
			this._passwordLabel.Size = new System.Drawing.Size(150, 20);
			this._passwordLabel.TabIndex = 7;
			this._passwordLabel.Text = "Password:";
			this._passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _passwordTextBox
			// 
			this._passwordTextBox.Location = new System.Drawing.Point(478, 91);
			this._passwordTextBox.Name = "_passwordTextBox";
			this._passwordTextBox.PasswordChar = '*';
			this._passwordTextBox.SecureText = secureString1;
			this._passwordTextBox.Size = new System.Drawing.Size(154, 20);
			this._passwordTextBox.TabIndex = 6;
			// 
			// _generalLabel
			// 
			this._generalLabel.AutoSize = true;
			this._generalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._generalLabel.Location = new System.Drawing.Point(239, 66);
			this._generalLabel.Name = "_generalLabel";
			this._generalLabel.Size = new System.Drawing.Size(63, 16);
			this._generalLabel.TabIndex = 8;
			this._generalLabel.Text = "General";
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BackColor = System.Drawing.Color.Silver;
			this.panel2.Location = new System.Drawing.Point(242, 125);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(471, 1);
			this.panel2.TabIndex = 9;
			// 
			// panel3
			// 
			this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel3.BackColor = System.Drawing.Color.Silver;
			this.panel3.Location = new System.Drawing.Point(242, 238);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(471, 1);
			this.panel3.TabIndex = 15;
			// 
			// _displayLabel
			// 
			this._displayLabel.AutoSize = true;
			this._displayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._displayLabel.Location = new System.Drawing.Point(239, 142);
			this._displayLabel.Name = "_displayLabel";
			this._displayLabel.Size = new System.Drawing.Size(61, 16);
			this._displayLabel.TabIndex = 14;
			this._displayLabel.Text = "Display";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(322, 203);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 20);
			this.label2.TabIndex = 13;
			this.label2.Text = "Color depth:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _resolutionLabel
			// 
			this._resolutionLabel.Location = new System.Drawing.Point(322, 140);
			this._resolutionLabel.Name = "_resolutionLabel";
			this._resolutionLabel.Size = new System.Drawing.Size(150, 20);
			this._resolutionLabel.TabIndex = 11;
			this._resolutionLabel.Text = "Resolution:";
			this._resolutionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _resolutionSlider
			// 
			this._resolutionSlider.Location = new System.Drawing.Point(470, 142);
			this._resolutionSlider.Name = "_resolutionSlider";
			this._resolutionSlider.Size = new System.Drawing.Size(240, 45);
			this._resolutionSlider.TabIndex = 16;
			this._resolutionSlider.ValueChanged += new System.EventHandler(this._resolutionSlider_ValueChanged);
			// 
			// _colorDepthDropdown
			// 
			this._colorDepthDropdown.FormattingEnabled = true;
			this._colorDepthDropdown.Items.AddRange(new object[] {
            "High Color (15 bit)",
            "High Color (16 bit)",
            "True Color (24 bit)",
            "Highest Quality (32 bit)"});
			this._colorDepthDropdown.Location = new System.Drawing.Point(480, 204);
			this._colorDepthDropdown.Name = "_colorDepthDropdown";
			this._colorDepthDropdown.Size = new System.Drawing.Size(152, 21);
			this._colorDepthDropdown.TabIndex = 17;
			// 
			// panel4
			// 
			this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel4.BackColor = System.Drawing.Color.Silver;
			this.panel4.Location = new System.Drawing.Point(242, 408);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(471, 1);
			this.panel4.TabIndex = 23;
			// 
			// _localResourcesLabel
			// 
			this._localResourcesLabel.AutoSize = true;
			this._localResourcesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._localResourcesLabel.Location = new System.Drawing.Point(239, 256);
			this._localResourcesLabel.Name = "_localResourcesLabel";
			this._localResourcesLabel.Size = new System.Drawing.Size(83, 32);
			this._localResourcesLabel.TabIndex = 22;
			this._localResourcesLabel.Text = "Local\r\nResources";
			// 
			// _remoteAudioRecordingLabel
			// 
			this._remoteAudioRecordingLabel.Location = new System.Drawing.Point(322, 280);
			this._remoteAudioRecordingLabel.Name = "_remoteAudioRecordingLabel";
			this._remoteAudioRecordingLabel.Size = new System.Drawing.Size(150, 20);
			this._remoteAudioRecordingLabel.TabIndex = 21;
			this._remoteAudioRecordingLabel.Text = "Remote audio recording:";
			this._remoteAudioRecordingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _remoteAudioPlaybackLabel
			// 
			this._remoteAudioPlaybackLabel.Location = new System.Drawing.Point(322, 254);
			this._remoteAudioPlaybackLabel.Name = "_remoteAudioPlaybackLabel";
			this._remoteAudioPlaybackLabel.Size = new System.Drawing.Size(150, 20);
			this._remoteAudioPlaybackLabel.TabIndex = 19;
			this._remoteAudioPlaybackLabel.Text = "Remote audio playback:";
			this._remoteAudioPlaybackLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _audioPlaybackDropdown
			// 
			this._audioPlaybackDropdown.FormattingEnabled = true;
			this._audioPlaybackDropdown.Items.AddRange(new object[] {
            "Play on this computer",
            "Do not play",
            "Play on remote computer"});
			this._audioPlaybackDropdown.Location = new System.Drawing.Point(480, 256);
			this._audioPlaybackDropdown.Name = "_audioPlaybackDropdown";
			this._audioPlaybackDropdown.Size = new System.Drawing.Size(182, 21);
			this._audioPlaybackDropdown.TabIndex = 24;
			// 
			// _audioRecordingDropdown
			// 
			this._audioRecordingDropdown.FormattingEnabled = true;
			this._audioRecordingDropdown.Items.AddRange(new object[] {
            "Record from this computer",
            "Do not record"});
			this._audioRecordingDropdown.Location = new System.Drawing.Point(480, 280);
			this._audioRecordingDropdown.Name = "_audioRecordingDropdown";
			this._audioRecordingDropdown.Size = new System.Drawing.Size(182, 21);
			this._audioRecordingDropdown.TabIndex = 25;
			// 
			// _windowsKeyDropdown
			// 
			this._windowsKeyDropdown.FormattingEnabled = true;
			this._windowsKeyDropdown.Items.AddRange(new object[] {
            "On this computer",
            "On the remote computer"});
			this._windowsKeyDropdown.Location = new System.Drawing.Point(480, 304);
			this._windowsKeyDropdown.Name = "_windowsKeyDropdown";
			this._windowsKeyDropdown.Size = new System.Drawing.Size(182, 21);
			this._windowsKeyDropdown.TabIndex = 27;
			// 
			// _windowsKeysLabel
			// 
			this._windowsKeysLabel.Location = new System.Drawing.Point(322, 304);
			this._windowsKeysLabel.Name = "_windowsKeysLabel";
			this._windowsKeysLabel.Size = new System.Drawing.Size(150, 20);
			this._windowsKeysLabel.TabIndex = 26;
			this._windowsKeysLabel.Text = "Windows key combos:";
			this._windowsKeysLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _localDevicesLabel
			// 
			this._localDevicesLabel.Location = new System.Drawing.Point(322, 330);
			this._localDevicesLabel.Name = "_localDevicesLabel";
			this._localDevicesLabel.Size = new System.Drawing.Size(150, 20);
			this._localDevicesLabel.TabIndex = 28;
			this._localDevicesLabel.Text = "Local devices:";
			this._localDevicesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _printersCheckbox
			// 
			this._printersCheckbox.AutoSize = true;
			this._printersCheckbox.Location = new System.Drawing.Point(480, 333);
			this._printersCheckbox.Name = "_printersCheckbox";
			this._printersCheckbox.Size = new System.Drawing.Size(61, 17);
			this._printersCheckbox.TabIndex = 29;
			this._printersCheckbox.Text = "Printers";
			this._printersCheckbox.UseVisualStyleBackColor = true;
			// 
			// _clipboardCheckbox
			// 
			this._clipboardCheckbox.AutoSize = true;
			this._clipboardCheckbox.Location = new System.Drawing.Point(480, 356);
			this._clipboardCheckbox.Name = "_clipboardCheckbox";
			this._clipboardCheckbox.Size = new System.Drawing.Size(70, 17);
			this._clipboardCheckbox.TabIndex = 30;
			this._clipboardCheckbox.Text = "Clipboard";
			this._clipboardCheckbox.UseVisualStyleBackColor = true;
			// 
			// _drivesCheckbox
			// 
			this._drivesCheckbox.AutoSize = true;
			this._drivesCheckbox.Location = new System.Drawing.Point(480, 379);
			this._drivesCheckbox.Name = "_drivesCheckbox";
			this._drivesCheckbox.Size = new System.Drawing.Size(56, 17);
			this._drivesCheckbox.TabIndex = 31;
			this._drivesCheckbox.Text = "Drives";
			this._drivesCheckbox.UseVisualStyleBackColor = true;
			// 
			// _desktopCompositionCheckbox
			// 
			this._desktopCompositionCheckbox.AutoSize = true;
			this._desktopCompositionCheckbox.Location = new System.Drawing.Point(480, 471);
			this._desktopCompositionCheckbox.Name = "_desktopCompositionCheckbox";
			this._desktopCompositionCheckbox.Size = new System.Drawing.Size(125, 17);
			this._desktopCompositionCheckbox.TabIndex = 42;
			this._desktopCompositionCheckbox.Text = "Desktop composition";
			this._desktopCompositionCheckbox.UseVisualStyleBackColor = true;
			// 
			// _fontSmoothingCheckbox
			// 
			this._fontSmoothingCheckbox.AutoSize = true;
			this._fontSmoothingCheckbox.Location = new System.Drawing.Point(480, 448);
			this._fontSmoothingCheckbox.Name = "_fontSmoothingCheckbox";
			this._fontSmoothingCheckbox.Size = new System.Drawing.Size(98, 17);
			this._fontSmoothingCheckbox.TabIndex = 41;
			this._fontSmoothingCheckbox.Text = "Font smoothing";
			this._fontSmoothingCheckbox.UseVisualStyleBackColor = true;
			// 
			// _desktopBackgroundCheckbox
			// 
			this._desktopBackgroundCheckbox.AutoSize = true;
			this._desktopBackgroundCheckbox.Location = new System.Drawing.Point(480, 425);
			this._desktopBackgroundCheckbox.Name = "_desktopBackgroundCheckbox";
			this._desktopBackgroundCheckbox.Size = new System.Drawing.Size(126, 17);
			this._desktopBackgroundCheckbox.TabIndex = 40;
			this._desktopBackgroundCheckbox.Text = "Desktop background";
			this._desktopBackgroundCheckbox.UseVisualStyleBackColor = true;
			// 
			// _allowTheFollowingLabel
			// 
			this._allowTheFollowingLabel.Location = new System.Drawing.Point(322, 422);
			this._allowTheFollowingLabel.Name = "_allowTheFollowingLabel";
			this._allowTheFollowingLabel.Size = new System.Drawing.Size(150, 20);
			this._allowTheFollowingLabel.TabIndex = 39;
			this._allowTheFollowingLabel.Text = "Allow the following:";
			this._allowTheFollowingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// _experienceLabel
			// 
			this._experienceLabel.AutoSize = true;
			this._experienceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._experienceLabel.Location = new System.Drawing.Point(239, 423);
			this._experienceLabel.Name = "_experienceLabel";
			this._experienceLabel.Size = new System.Drawing.Size(86, 16);
			this._experienceLabel.TabIndex = 34;
			this._experienceLabel.Text = "Experience";
			// 
			// _windowContentsWhileDraggingCheckbox
			// 
			this._windowContentsWhileDraggingCheckbox.AutoSize = true;
			this._windowContentsWhileDraggingCheckbox.Location = new System.Drawing.Point(480, 494);
			this._windowContentsWhileDraggingCheckbox.Name = "_windowContentsWhileDraggingCheckbox";
			this._windowContentsWhileDraggingCheckbox.Size = new System.Drawing.Size(207, 17);
			this._windowContentsWhileDraggingCheckbox.TabIndex = 43;
			this._windowContentsWhileDraggingCheckbox.Text = "Show window contents while dragging";
			this._windowContentsWhileDraggingCheckbox.UseVisualStyleBackColor = true;
			// 
			// _menuAnimationCheckbox
			// 
			this._menuAnimationCheckbox.AutoSize = true;
			this._menuAnimationCheckbox.Location = new System.Drawing.Point(480, 517);
			this._menuAnimationCheckbox.Name = "_menuAnimationCheckbox";
			this._menuAnimationCheckbox.Size = new System.Drawing.Size(161, 17);
			this._menuAnimationCheckbox.TabIndex = 44;
			this._menuAnimationCheckbox.Text = "Menu and window animation";
			this._menuAnimationCheckbox.UseVisualStyleBackColor = true;
			// 
			// _visualStylesCheckbox
			// 
			this._visualStylesCheckbox.AutoSize = true;
			this._visualStylesCheckbox.Location = new System.Drawing.Point(480, 540);
			this._visualStylesCheckbox.Name = "_visualStylesCheckbox";
			this._visualStylesCheckbox.Size = new System.Drawing.Size(83, 17);
			this._visualStylesCheckbox.TabIndex = 45;
			this._visualStylesCheckbox.Text = "Visual styles";
			this._visualStylesCheckbox.UseVisualStyleBackColor = true;
			// 
			// _bitmapCachingCheckbox
			// 
			this._bitmapCachingCheckbox.AutoSize = true;
			this._bitmapCachingCheckbox.Location = new System.Drawing.Point(480, 563);
			this._bitmapCachingCheckbox.Name = "_bitmapCachingCheckbox";
			this._bitmapCachingCheckbox.Size = new System.Drawing.Size(147, 17);
			this._bitmapCachingCheckbox.TabIndex = 46;
			this._bitmapCachingCheckbox.Text = "Persistent bitmap caching";
			this._bitmapCachingCheckbox.UseVisualStyleBackColor = true;
			// 
			// _resolutionSliderLabel
			// 
			this._resolutionSliderLabel.Location = new System.Drawing.Point(478, 178);
			this._resolutionSliderLabel.Name = "_resolutionSliderLabel";
			this._resolutionSliderLabel.Size = new System.Drawing.Size(223, 23);
			this._resolutionSliderLabel.TabIndex = 47;
			this._resolutionSliderLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// OptionsWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.ClientSize = new System.Drawing.Size(741, 682);
			this.Controls.Add(this._resolutionSliderLabel);
			this.Controls.Add(this._bitmapCachingCheckbox);
			this.Controls.Add(this._visualStylesCheckbox);
			this.Controls.Add(this._menuAnimationCheckbox);
			this.Controls.Add(this._windowContentsWhileDraggingCheckbox);
			this.Controls.Add(this._desktopCompositionCheckbox);
			this.Controls.Add(this._fontSmoothingCheckbox);
			this.Controls.Add(this._desktopBackgroundCheckbox);
			this.Controls.Add(this._allowTheFollowingLabel);
			this.Controls.Add(this._experienceLabel);
			this.Controls.Add(this._drivesCheckbox);
			this.Controls.Add(this._clipboardCheckbox);
			this.Controls.Add(this._printersCheckbox);
			this.Controls.Add(this._localDevicesLabel);
			this.Controls.Add(this._windowsKeyDropdown);
			this.Controls.Add(this._windowsKeysLabel);
			this.Controls.Add(this._audioRecordingDropdown);
			this.Controls.Add(this._audioPlaybackDropdown);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this._localResourcesLabel);
			this.Controls.Add(this._remoteAudioRecordingLabel);
			this.Controls.Add(this._remoteAudioPlaybackLabel);
			this.Controls.Add(this._colorDepthDropdown);
			this.Controls.Add(this._resolutionSlider);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this._displayLabel);
			this.Controls.Add(this._generalLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._passwordLabel);
			this.Controls.Add(this._resolutionLabel);
			this.Controls.Add(this._passwordTextBox);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._selectedOptionCategoryTitle);
			this.Controls.Add(this._userNameLabel);
			this.Controls.Add(this._userNameTextBox);
			this.Controls.Add(this._topBorderPanel);
			this.Controls.Add(this._sidebarContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "OptionsWindow";
			this.Text = "Options";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OptionsWindow_FormClosed);
			this.Load += new System.EventHandler(this.OptionsWindow_Load);
			this._sidebarContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._resolutionSlider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _sidebarContainer;
        private System.Windows.Forms.Label _optionsLabel;
        private System.Windows.Forms.Label _rdpDefaultsLabel;
        private System.Windows.Forms.Panel _topBorderPanel;
        private System.Windows.Forms.TextBox _userNameTextBox;
        private System.Windows.Forms.Label _userNameLabel;
        private System.Windows.Forms.Label _selectedOptionCategoryTitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _passwordLabel;
        private SecurePasswordTextBox.SecureTextBox _passwordTextBox;
        private System.Windows.Forms.Label _generalLabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label _displayLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label _resolutionLabel;
        private System.Windows.Forms.TrackBar _resolutionSlider;
        private System.Windows.Forms.ComboBox _colorDepthDropdown;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label _localResourcesLabel;
        private System.Windows.Forms.Label _remoteAudioRecordingLabel;
        private System.Windows.Forms.Label _remoteAudioPlaybackLabel;
        private System.Windows.Forms.ComboBox _audioPlaybackDropdown;
        private System.Windows.Forms.ComboBox _audioRecordingDropdown;
        private System.Windows.Forms.ComboBox _windowsKeyDropdown;
        private System.Windows.Forms.Label _windowsKeysLabel;
        private System.Windows.Forms.Label _localDevicesLabel;
        private System.Windows.Forms.CheckBox _printersCheckbox;
        private System.Windows.Forms.CheckBox _clipboardCheckbox;
        private System.Windows.Forms.CheckBox _drivesCheckbox;
        private System.Windows.Forms.CheckBox _desktopCompositionCheckbox;
        private System.Windows.Forms.CheckBox _fontSmoothingCheckbox;
        private System.Windows.Forms.CheckBox _desktopBackgroundCheckbox;
        private System.Windows.Forms.Label _allowTheFollowingLabel;
        private System.Windows.Forms.Label _experienceLabel;
        private System.Windows.Forms.CheckBox _windowContentsWhileDraggingCheckbox;
        private System.Windows.Forms.CheckBox _menuAnimationCheckbox;
        private System.Windows.Forms.CheckBox _visualStylesCheckbox;
        private System.Windows.Forms.CheckBox _bitmapCachingCheckbox;
        private System.Windows.Forms.Label _resolutionSliderLabel;
    }
}