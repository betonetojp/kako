namespace kako
{
    partial class FormSetting
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSetting));
            textBoxNsec = new TextBox();
            trackBarOpacity = new TrackBar();
            checkBoxTopMost = new CheckBox();
            label1 = new Label();
            checkBoxAddClient = new CheckBox();
            label4 = new Label();
            linkLabelIcons8 = new LinkLabel();
            labelOpacity = new Label();
            checkBoxShowOnlyFollowees = new CheckBox();
            label3 = new Label();
            linkLabelVersion = new LinkLabel();
            checkBoxMinimizeToTray = new CheckBox();
            label2 = new Label();
            textBoxNpub = new TextBox();
            buttonLogOut = new Button();
            toolTipLogOut = new ToolTip(components);
            textBoxDirector = new TextBox();
            label5 = new Label();
            label6 = new Label();
            textBoxReplyCommands = new TextBox();
            label7 = new Label();
            checkBoxMentionEveryHour = new CheckBox();
            numericUpDownMentionMinutes = new NumericUpDown();
            label8 = new Label();
            textBoxCallCommand = new TextBox();
            checkBoxOpenMode = new CheckBox();
            numericUpDownCallReplyLimit = new NumericUpDown();
            label9 = new Label();
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMentionMinutes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCallReplyLimit).BeginInit();
            SuspendLayout();
            // 
            // textBoxNsec
            // 
            textBoxNsec.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxNsec.BorderStyle = BorderStyle.FixedSingle;
            textBoxNsec.ImeMode = ImeMode.Disable;
            textBoxNsec.Location = new Point(82, 241);
            textBoxNsec.MaxLength = 256;
            textBoxNsec.Name = "textBoxNsec";
            textBoxNsec.PasswordChar = '*';
            textBoxNsec.PlaceholderText = "nsec1...";
            textBoxNsec.Size = new Size(261, 23);
            textBoxNsec.TabIndex = 10;
            textBoxNsec.Leave += TextBoxNsec_Leave;
            // 
            // trackBarOpacity
            // 
            trackBarOpacity.Location = new Point(252, 31);
            trackBarOpacity.Maximum = 100;
            trackBarOpacity.Minimum = 20;
            trackBarOpacity.Name = "trackBarOpacity";
            trackBarOpacity.Size = new Size(120, 45);
            trackBarOpacity.TabIndex = 2;
            trackBarOpacity.TickFrequency = 20;
            trackBarOpacity.Value = 100;
            trackBarOpacity.Scroll += TrackBarOpacity_Scroll;
            // 
            // checkBoxTopMost
            // 
            checkBoxTopMost.AutoSize = true;
            checkBoxTopMost.Location = new Point(12, 12);
            checkBoxTopMost.Name = "checkBoxTopMost";
            checkBoxTopMost.Size = new Size(101, 19);
            checkBoxTopMost.TabIndex = 1;
            checkBoxTopMost.Text = "Always on top";
            checkBoxTopMost.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(252, 13);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 0;
            label1.Text = "Opacity";
            // 
            // checkBoxAddClient
            // 
            checkBoxAddClient.AutoSize = true;
            checkBoxAddClient.ForeColor = SystemColors.ControlText;
            checkBoxAddClient.Location = new Point(12, 62);
            checkBoxAddClient.Name = "checkBoxAddClient";
            checkBoxAddClient.Size = new Size(100, 19);
            checkBoxAddClient.TabIndex = 4;
            checkBoxAddClient.Text = "Add client tag";
            checkBoxAddClient.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.GrayText;
            label4.Location = new Point(199, 337);
            label4.Name = "label4";
            label4.Size = new Size(126, 15);
            label4.TabIndex = 0;
            label4.Text = "Monochrome icons by";
            // 
            // linkLabelIcons8
            // 
            linkLabelIcons8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelIcons8.AutoSize = true;
            linkLabelIcons8.Location = new Point(331, 337);
            linkLabelIcons8.Name = "linkLabelIcons8";
            linkLabelIcons8.Size = new Size(41, 15);
            linkLabelIcons8.TabIndex = 17;
            linkLabelIcons8.TabStop = true;
            linkLabelIcons8.Text = "Icons8";
            linkLabelIcons8.LinkClicked += LinkLabelIcons8_LinkClicked;
            // 
            // labelOpacity
            // 
            labelOpacity.Location = new Point(331, 13);
            labelOpacity.Name = "labelOpacity";
            labelOpacity.Size = new Size(41, 15);
            labelOpacity.TabIndex = 0;
            labelOpacity.Text = "100%";
            labelOpacity.TextAlign = ContentAlignment.TopRight;
            // 
            // checkBoxShowOnlyFollowees
            // 
            checkBoxShowOnlyFollowees.AutoSize = true;
            checkBoxShowOnlyFollowees.Location = new Point(12, 117);
            checkBoxShowOnlyFollowees.Name = "checkBoxShowOnlyFollowees";
            checkBoxShowOnlyFollowees.Size = new Size(134, 19);
            checkBoxShowOnlyFollowees.TabIndex = 6;
            checkBoxShowOnlyFollowees.Text = "Show only followees";
            checkBoxShowOnlyFollowees.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 245);
            label3.Name = "label3";
            label3.Size = new Size(64, 15);
            label3.TabIndex = 0;
            label3.Text = "Private key";
            // 
            // linkLabelVersion
            // 
            linkLabelVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelVersion.AutoSize = true;
            linkLabelVersion.Location = new Point(12, 337);
            linkLabelVersion.Name = "linkLabelVersion";
            linkLabelVersion.Size = new Size(37, 15);
            linkLabelVersion.TabIndex = 16;
            linkLabelVersion.TabStop = true;
            linkLabelVersion.Text = "v0.2.9";
            linkLabelVersion.LinkClicked += LinkLabelVersion_LinkClicked;
            // 
            // checkBoxMinimizeToTray
            // 
            checkBoxMinimizeToTray.AutoSize = true;
            checkBoxMinimizeToTray.Location = new Point(12, 37);
            checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            checkBoxMinimizeToTray.Size = new Size(150, 19);
            checkBoxMinimizeToTray.TabIndex = 3;
            checkBoxMinimizeToTray.Text = "Minimize to system tray";
            checkBoxMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 272);
            label2.Name = "label2";
            label2.Size = new Size(61, 15);
            label2.TabIndex = 15;
            label2.Text = "Public key";
            // 
            // textBoxNpub
            // 
            textBoxNpub.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxNpub.BorderStyle = BorderStyle.FixedSingle;
            textBoxNpub.Location = new Point(82, 270);
            textBoxNpub.Name = "textBoxNpub";
            textBoxNpub.PlaceholderText = "npub1...";
            textBoxNpub.ReadOnly = true;
            textBoxNpub.Size = new Size(290, 23);
            textBoxNpub.TabIndex = 12;
            // 
            // buttonLogOut
            // 
            buttonLogOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLogOut.Image = Properties.Resources.icons8_log_out_16;
            buttonLogOut.Location = new Point(349, 241);
            buttonLogOut.Name = "buttonLogOut";
            buttonLogOut.Size = new Size(23, 23);
            buttonLogOut.TabIndex = 11;
            toolTipLogOut.SetToolTip(buttonLogOut, "Log out");
            buttonLogOut.UseVisualStyleBackColor = true;
            buttonLogOut.Click += ButtonLogOut_Click;
            // 
            // textBoxDirector
            // 
            textBoxDirector.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxDirector.BorderStyle = BorderStyle.FixedSingle;
            textBoxDirector.Location = new Point(114, 87);
            textBoxDirector.Name = "textBoxDirector";
            textBoxDirector.PlaceholderText = "npub1...";
            textBoxDirector.Size = new Size(258, 23);
            textBoxDirector.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 89);
            label5.Name = "label5";
            label5.Size = new Size(88, 15);
            label5.TabIndex = 29;
            label5.Text = "Director's npub";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(11, 144);
            label6.Name = "label6";
            label6.Size = new Size(97, 15);
            label6.TabIndex = 0;
            label6.Text = "Reply commands";
            // 
            // textBoxReplyCommands
            // 
            textBoxReplyCommands.BorderStyle = BorderStyle.FixedSingle;
            textBoxReplyCommands.Location = new Point(114, 142);
            textBoxReplyCommands.Multiline = true;
            textBoxReplyCommands.Name = "textBoxReplyCommands";
            textBoxReplyCommands.ScrollBars = ScrollBars.Vertical;
            textBoxReplyCommands.Size = new Size(258, 64);
            textBoxReplyCommands.TabIndex = 7;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(212, 301);
            label7.Name = "label7";
            label7.Size = new Size(49, 15);
            label7.TabIndex = 0;
            label7.Text = "minutes";
            // 
            // checkBoxMentionEveryHour
            // 
            checkBoxMentionEveryHour.AutoSize = true;
            checkBoxMentionEveryHour.Location = new Point(12, 300);
            checkBoxMentionEveryHour.Name = "checkBoxMentionEveryHour";
            checkBoxMentionEveryHour.Size = new Size(143, 19);
            checkBoxMentionEveryHour.TabIndex = 13;
            checkBoxMentionEveryHour.Text = "Mention every hour at";
            checkBoxMentionEveryHour.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMentionMinutes
            // 
            numericUpDownMentionMinutes.Location = new Point(161, 299);
            numericUpDownMentionMinutes.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            numericUpDownMentionMinutes.Name = "numericUpDownMentionMinutes";
            numericUpDownMentionMinutes.Size = new Size(45, 23);
            numericUpDownMentionMinutes.TabIndex = 14;
            numericUpDownMentionMinutes.TextAlign = HorizontalAlignment.Center;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(12, 214);
            label8.Name = "label8";
            label8.Size = new Size(82, 15);
            label8.TabIndex = 0;
            label8.Text = "Call command";
            // 
            // textBoxCallCommand
            // 
            textBoxCallCommand.BorderStyle = BorderStyle.FixedSingle;
            textBoxCallCommand.Location = new Point(114, 212);
            textBoxCallCommand.Name = "textBoxCallCommand";
            textBoxCallCommand.Size = new Size(164, 23);
            textBoxCallCommand.TabIndex = 8;
            // 
            // checkBoxOpenMode
            // 
            checkBoxOpenMode.AutoSize = true;
            checkBoxOpenMode.Location = new Point(284, 213);
            checkBoxOpenMode.Name = "checkBoxOpenMode";
            checkBoxOpenMode.Size = new Size(88, 19);
            checkBoxOpenMode.TabIndex = 9;
            checkBoxOpenMode.Text = "Open mode";
            checkBoxOpenMode.UseVisualStyleBackColor = true;
            // 
            // numericUpDownCallReplyLimit
            // 
            numericUpDownCallReplyLimit.Location = new Point(327, 299);
            numericUpDownCallReplyLimit.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            numericUpDownCallReplyLimit.Name = "numericUpDownCallReplyLimit";
            numericUpDownCallReplyLimit.Size = new Size(45, 23);
            numericUpDownCallReplyLimit.TabIndex = 15;
            numericUpDownCallReplyLimit.TextAlign = HorizontalAlignment.Center;
            numericUpDownCallReplyLimit.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(272, 301);
            label9.Name = "label9";
            label9.Size = new Size(49, 15);
            label9.TabIndex = 0;
            label9.Text = "Stamina";
            // 
            // FormSetting
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(384, 361);
            Controls.Add(label9);
            Controls.Add(checkBoxOpenMode);
            Controls.Add(textBoxCallCommand);
            Controls.Add(label8);
            Controls.Add(numericUpDownCallReplyLimit);
            Controls.Add(numericUpDownMentionMinutes);
            Controls.Add(checkBoxMentionEveryHour);
            Controls.Add(label7);
            Controls.Add(textBoxReplyCommands);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(buttonLogOut);
            Controls.Add(textBoxDirector);
            Controls.Add(textBoxNpub);
            Controls.Add(label2);
            Controls.Add(checkBoxMinimizeToTray);
            Controls.Add(linkLabelVersion);
            Controls.Add(label3);
            Controls.Add(checkBoxShowOnlyFollowees);
            Controls.Add(labelOpacity);
            Controls.Add(linkLabelIcons8);
            Controls.Add(label4);
            Controls.Add(checkBoxAddClient);
            Controls.Add(label1);
            Controls.Add(checkBoxTopMost);
            Controls.Add(trackBarOpacity);
            Controls.Add(textBoxNsec);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(400, 400);
            Name = "FormSetting";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Setting";
            TopMost = true;
            Load += FormSetting_Load;
            Shown += FormSetting_Shown;
            KeyDown += FormSetting_KeyDown;
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMentionMinutes).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCallReplyLimit).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        internal TextBox textBoxNsec;
        internal TrackBar trackBarOpacity;
        internal CheckBox checkBoxTopMost;
        private Label label1;
        internal CheckBox checkBoxAddClient;
        private Label label4;
        private LinkLabel linkLabelIcons8;
        private Label labelOpacity;
        internal CheckBox checkBoxShowOnlyFollowees;
        private Label label3;
        private LinkLabel linkLabelVersion;
        internal CheckBox checkBoxMinimizeToTray;
        private Label label2;
        internal TextBox textBoxNpub;
        private Button buttonLogOut;
        private ToolTip toolTipLogOut;
        internal TextBox textBoxDirector;
        private Label label5;
        private Label label6;
        internal TextBox textBoxReplyCommands;
        private Label label7;
        internal CheckBox checkBoxMentionEveryHour;
        internal NumericUpDown numericUpDownMentionMinutes;
        private NumericUpDown numericUpDown1;
        private Label label8;
        internal TextBox textBoxCallCommand;
        internal CheckBox checkBoxOpenMode;
        internal NumericUpDown numericUpDownCallReplyLimit;
        private Label label9;
    }
}