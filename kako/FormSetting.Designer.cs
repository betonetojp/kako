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
            textBoxForceCommands = new TextBox();
            label7 = new Label();
            checkBoxSummarizeEveryHour = new CheckBox();
            numericUpDownSummarizeMinutes = new NumericUpDown();
            label8 = new Label();
            textBoxCallCommands = new TextBox();
            checkBoxOpenMode = new CheckBox();
            numericUpDownCallReplyLimit = new NumericUpDown();
            label9 = new Label();
            checkBoxMentionMode = new CheckBox();
            checkBoxUsePetname = new CheckBox();
            label10 = new Label();
            checkBoxSummarizeByEventCount = new CheckBox();
            numericUpDownEventThreshold = new NumericUpDown();
            checkBoxAddNostrNpub1 = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownSummarizeMinutes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCallReplyLimit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownEventThreshold).BeginInit();
            SuspendLayout();
            // 
            // textBoxNsec
            // 
            textBoxNsec.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxNsec.BorderStyle = BorderStyle.FixedSingle;
            textBoxNsec.ImeMode = ImeMode.Disable;
            textBoxNsec.Location = new Point(115, 308);
            textBoxNsec.MaxLength = 256;
            textBoxNsec.Name = "textBoxNsec";
            textBoxNsec.PasswordChar = '*';
            textBoxNsec.PlaceholderText = "nsec1...";
            textBoxNsec.Size = new Size(268, 23);
            textBoxNsec.TabIndex = 18;
            textBoxNsec.Leave += TextBoxNsec_Leave;
            // 
            // trackBarOpacity
            // 
            trackBarOpacity.Location = new Point(292, 36);
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
            label1.Location = new Point(292, 13);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 0;
            label1.Text = "Opacity";
            // 
            // checkBoxAddClient
            // 
            checkBoxAddClient.AutoSize = true;
            checkBoxAddClient.Checked = true;
            checkBoxAddClient.CheckState = CheckState.Checked;
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
            label4.Location = new Point(199, 377);
            label4.Name = "label4";
            label4.Size = new Size(126, 15);
            label4.TabIndex = 0;
            label4.Text = "Monochrome icons by";
            // 
            // linkLabelIcons8
            // 
            linkLabelIcons8.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelIcons8.AutoSize = true;
            linkLabelIcons8.Location = new Point(331, 377);
            linkLabelIcons8.Name = "linkLabelIcons8";
            linkLabelIcons8.Size = new Size(41, 15);
            linkLabelIcons8.TabIndex = 22;
            linkLabelIcons8.TabStop = true;
            linkLabelIcons8.Text = "Icons8";
            linkLabelIcons8.LinkClicked += LinkLabelIcons8_LinkClicked;
            // 
            // labelOpacity
            // 
            labelOpacity.Location = new Point(371, 13);
            labelOpacity.Name = "labelOpacity";
            labelOpacity.Size = new Size(41, 15);
            labelOpacity.TabIndex = 0;
            labelOpacity.Text = "100%";
            labelOpacity.TextAlign = ContentAlignment.TopRight;
            // 
            // checkBoxShowOnlyFollowees
            // 
            checkBoxShowOnlyFollowees.AutoSize = true;
            checkBoxShowOnlyFollowees.Location = new Point(12, 116);
            checkBoxShowOnlyFollowees.Name = "checkBoxShowOnlyFollowees";
            checkBoxShowOnlyFollowees.Size = new Size(134, 19);
            checkBoxShowOnlyFollowees.TabIndex = 6;
            checkBoxShowOnlyFollowees.Text = "Show only followees";
            checkBoxShowOnlyFollowees.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(45, 310);
            label3.Name = "label3";
            label3.Size = new Size(64, 15);
            label3.TabIndex = 0;
            label3.Text = "Private key";
            // 
            // linkLabelVersion
            // 
            linkLabelVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelVersion.AutoSize = true;
            linkLabelVersion.Location = new Point(12, 377);
            linkLabelVersion.Name = "linkLabelVersion";
            linkLabelVersion.Size = new Size(37, 15);
            linkLabelVersion.TabIndex = 21;
            linkLabelVersion.TabStop = true;
            linkLabelVersion.Text = "v0.3.7";
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
            label2.Location = new Point(48, 339);
            label2.Name = "label2";
            label2.Size = new Size(61, 15);
            label2.TabIndex = 15;
            label2.Text = "Public key";
            // 
            // textBoxNpub
            // 
            textBoxNpub.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxNpub.BorderStyle = BorderStyle.FixedSingle;
            textBoxNpub.Location = new Point(115, 337);
            textBoxNpub.Name = "textBoxNpub";
            textBoxNpub.PlaceholderText = "npub1...";
            textBoxNpub.ReadOnly = true;
            textBoxNpub.Size = new Size(297, 23);
            textBoxNpub.TabIndex = 20;
            // 
            // buttonLogOut
            // 
            buttonLogOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLogOut.Image = Properties.Resources.icons8_log_out_16;
            buttonLogOut.Location = new Point(389, 308);
            buttonLogOut.Name = "buttonLogOut";
            buttonLogOut.Size = new Size(23, 23);
            buttonLogOut.TabIndex = 19;
            toolTipLogOut.SetToolTip(buttonLogOut, "Log out");
            buttonLogOut.UseVisualStyleBackColor = true;
            buttonLogOut.Click += ButtonLogOut_Click;
            // 
            // textBoxDirector
            // 
            textBoxDirector.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxDirector.BorderStyle = BorderStyle.FixedSingle;
            textBoxDirector.Location = new Point(115, 87);
            textBoxDirector.Name = "textBoxDirector";
            textBoxDirector.PlaceholderText = "npub1...";
            textBoxDirector.Size = new Size(297, 23);
            textBoxDirector.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(21, 89);
            label5.Name = "label5";
            label5.Size = new Size(88, 15);
            label5.TabIndex = 29;
            label5.Text = "Director's npub";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 200);
            label6.Name = "label6";
            label6.Size = new Size(97, 15);
            label6.TabIndex = 0;
            label6.Text = "Force commands";
            // 
            // textBoxForceCommands
            // 
            textBoxForceCommands.BorderStyle = BorderStyle.FixedSingle;
            textBoxForceCommands.Location = new Point(115, 198);
            textBoxForceCommands.Multiline = true;
            textBoxForceCommands.Name = "textBoxForceCommands";
            textBoxForceCommands.ScrollBars = ScrollBars.Vertical;
            textBoxForceCommands.Size = new Size(149, 49);
            textBoxForceCommands.TabIndex = 14;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(224, 143);
            label7.Name = "label7";
            label7.Size = new Size(49, 15);
            label7.TabIndex = 0;
            label7.Text = "minutes";
            // 
            // checkBoxSummarizeEveryHour
            // 
            checkBoxSummarizeEveryHour.AutoSize = true;
            checkBoxSummarizeEveryHour.Location = new Point(12, 142);
            checkBoxSummarizeEveryHour.Name = "checkBoxSummarizeEveryHour";
            checkBoxSummarizeEveryHour.Size = new Size(155, 19);
            checkBoxSummarizeEveryHour.TabIndex = 8;
            checkBoxSummarizeEveryHour.Text = "Summarize every hour at";
            checkBoxSummarizeEveryHour.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSummarizeMinutes
            // 
            numericUpDownSummarizeMinutes.Location = new Point(173, 141);
            numericUpDownSummarizeMinutes.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            numericUpDownSummarizeMinutes.Name = "numericUpDownSummarizeMinutes";
            numericUpDownSummarizeMinutes.Size = new Size(45, 23);
            numericUpDownSummarizeMinutes.TabIndex = 9;
            numericUpDownSummarizeMinutes.TextAlign = HorizontalAlignment.Center;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(22, 255);
            label8.Name = "label8";
            label8.Size = new Size(87, 15);
            label8.TabIndex = 0;
            label8.Text = "Call commands";
            // 
            // textBoxCallCommands
            // 
            textBoxCallCommands.BorderStyle = BorderStyle.FixedSingle;
            textBoxCallCommands.Location = new Point(115, 253);
            textBoxCallCommands.Multiline = true;
            textBoxCallCommands.Name = "textBoxCallCommands";
            textBoxCallCommands.ScrollBars = ScrollBars.Vertical;
            textBoxCallCommands.Size = new Size(149, 49);
            textBoxCallCommands.TabIndex = 15;
            // 
            // checkBoxOpenMode
            // 
            checkBoxOpenMode.AutoSize = true;
            checkBoxOpenMode.Location = new Point(270, 254);
            checkBoxOpenMode.Name = "checkBoxOpenMode";
            checkBoxOpenMode.Size = new Size(88, 19);
            checkBoxOpenMode.TabIndex = 16;
            checkBoxOpenMode.Text = "Open mode";
            checkBoxOpenMode.UseVisualStyleBackColor = true;
            // 
            // numericUpDownCallReplyLimit
            // 
            numericUpDownCallReplyLimit.Location = new Point(325, 279);
            numericUpDownCallReplyLimit.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            numericUpDownCallReplyLimit.Name = "numericUpDownCallReplyLimit";
            numericUpDownCallReplyLimit.Size = new Size(47, 23);
            numericUpDownCallReplyLimit.TabIndex = 17;
            numericUpDownCallReplyLimit.TextAlign = HorizontalAlignment.Center;
            numericUpDownCallReplyLimit.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(270, 281);
            label9.Name = "label9";
            label9.Size = new Size(49, 15);
            label9.TabIndex = 0;
            label9.Text = "Stamina";
            // 
            // checkBoxMentionMode
            // 
            checkBoxMentionMode.AutoSize = true;
            checkBoxMentionMode.Location = new Point(279, 142);
            checkBoxMentionMode.Name = "checkBoxMentionMode";
            checkBoxMentionMode.Size = new Size(71, 19);
            checkBoxMentionMode.TabIndex = 10;
            checkBoxMentionMode.Text = "Mention";
            checkBoxMentionMode.UseVisualStyleBackColor = true;
            // 
            // checkBoxUsePetname
            // 
            checkBoxUsePetname.AutoSize = true;
            checkBoxUsePetname.Checked = true;
            checkBoxUsePetname.CheckState = CheckState.Checked;
            checkBoxUsePetname.Location = new Point(152, 116);
            checkBoxUsePetname.Name = "checkBoxUsePetname";
            checkBoxUsePetname.Size = new Size(94, 19);
            checkBoxUsePetname.TabIndex = 7;
            checkBoxUsePetname.Text = "Use petname";
            checkBoxUsePetname.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(224, 172);
            label10.Name = "label10";
            label10.Size = new Size(41, 15);
            label10.TabIndex = 0;
            label10.Text = "events";
            // 
            // checkBoxSummarizeByEventCount
            // 
            checkBoxSummarizeByEventCount.AutoSize = true;
            checkBoxSummarizeByEventCount.Location = new Point(12, 171);
            checkBoxSummarizeByEventCount.Name = "checkBoxSummarizeByEventCount";
            checkBoxSummarizeByEventCount.Size = new Size(114, 19);
            checkBoxSummarizeByEventCount.TabIndex = 11;
            checkBoxSummarizeByEventCount.Text = "Summarize every";
            checkBoxSummarizeByEventCount.UseVisualStyleBackColor = true;
            // 
            // numericUpDownEventThreshold
            // 
            numericUpDownEventThreshold.Location = new Point(132, 170);
            numericUpDownEventThreshold.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericUpDownEventThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownEventThreshold.Name = "numericUpDownEventThreshold";
            numericUpDownEventThreshold.Size = new Size(86, 23);
            numericUpDownEventThreshold.TabIndex = 12;
            numericUpDownEventThreshold.TextAlign = HorizontalAlignment.Center;
            numericUpDownEventThreshold.Value = new decimal(new int[] { 9999, 0, 0, 0 });
            // 
            // checkBoxAddNostrNpub1
            // 
            checkBoxAddNostrNpub1.AutoSize = true;
            checkBoxAddNostrNpub1.Location = new Point(279, 171);
            checkBoxAddNostrNpub1.Name = "checkBoxAddNostrNpub1";
            checkBoxAddNostrNpub1.Size = new Size(124, 19);
            checkBoxAddNostrNpub1.TabIndex = 13;
            checkBoxAddNostrNpub1.Text = "Add nostr:npub1...";
            checkBoxAddNostrNpub1.UseVisualStyleBackColor = true;
            // 
            // FormSetting
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(424, 401);
            Controls.Add(checkBoxAddNostrNpub1);
            Controls.Add(checkBoxUsePetname);
            Controls.Add(checkBoxMentionMode);
            Controls.Add(label9);
            Controls.Add(checkBoxOpenMode);
            Controls.Add(textBoxCallCommands);
            Controls.Add(label8);
            Controls.Add(numericUpDownCallReplyLimit);
            Controls.Add(numericUpDownEventThreshold);
            Controls.Add(numericUpDownSummarizeMinutes);
            Controls.Add(checkBoxSummarizeByEventCount);
            Controls.Add(label10);
            Controls.Add(checkBoxSummarizeEveryHour);
            Controls.Add(label7);
            Controls.Add(textBoxForceCommands);
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
            MinimumSize = new Size(440, 440);
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
            ((System.ComponentModel.ISupportInitialize)numericUpDownSummarizeMinutes).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCallReplyLimit).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownEventThreshold).EndInit();
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
        internal TextBox textBoxForceCommands;
        private Label label7;
        internal CheckBox checkBoxSummarizeEveryHour;
        internal NumericUpDown numericUpDownSummarizeMinutes;
        private NumericUpDown numericUpDown1;
        private Label label8;
        internal TextBox textBoxCallCommands;
        internal CheckBox checkBoxOpenMode;
        internal NumericUpDown numericUpDownCallReplyLimit;
        private Label label9;
        internal CheckBox checkBoxMentionMode;
        internal CheckBox checkBoxUsePetname;
        private Label label10;
        internal CheckBox checkBoxSummarizeByEventCount;
        internal NumericUpDown numericUpDownEventThreshold;
        internal CheckBox checkBoxAddNostrNpub1;
    }
}