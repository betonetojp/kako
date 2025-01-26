namespace kako
{
    partial class FormManiacs
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormManiacs));
            dataGridViewUsers = new DataGridView();
            mute = new DataGridViewCheckBoxColumn();
            last_activity = new DataGridViewTextBoxColumn();
            petname = new DataGridViewTextBoxColumn();
            display_name = new DataGridViewTextBoxColumn();
            name = new DataGridViewTextBoxColumn();
            pubkey = new DataGridViewTextBoxColumn();
            nip05 = new DataGridViewTextBoxColumn();
            picture = new DataGridViewTextBoxColumn();
            created_at = new DataGridViewTextBoxColumn();
            buttonSave = new Button();
            buttonDelete = new Button();
            buttonReload = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewUsers).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewUsers
            // 
            dataGridViewUsers.AllowUserToAddRows = false;
            dataGridViewUsers.AllowUserToDeleteRows = false;
            dataGridViewUsers.AllowUserToResizeRows = false;
            dataGridViewUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewUsers.Columns.AddRange(new DataGridViewColumn[] { mute, last_activity, petname, display_name, name, pubkey, nip05, picture, created_at });
            dataGridViewUsers.Location = new Point(12, 12);
            dataGridViewUsers.Name = "dataGridViewUsers";
            dataGridViewUsers.RowHeadersVisible = false;
            dataGridViewUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewUsers.ShowCellToolTips = false;
            dataGridViewUsers.Size = new Size(440, 291);
            dataGridViewUsers.StandardTab = true;
            dataGridViewUsers.TabIndex = 1;
            // 
            // mute
            // 
            mute.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            mute.HeaderText = "Mute";
            mute.MinimumWidth = 20;
            mute.Name = "mute";
            mute.SortMode = DataGridViewColumnSortMode.Automatic;
            mute.Width = 60;
            // 
            // last_activity
            // 
            last_activity.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            last_activity.HeaderText = "Last activity";
            last_activity.MinimumWidth = 20;
            last_activity.Name = "last_activity";
            last_activity.ReadOnly = true;
            last_activity.Width = 94;
            // 
            // petname
            // 
            petname.HeaderText = "petname";
            petname.MinimumWidth = 20;
            petname.Name = "petname";
            // 
            // display_name
            // 
            display_name.HeaderText = "display_name";
            display_name.MinimumWidth = 20;
            display_name.Name = "display_name";
            display_name.ReadOnly = true;
            // 
            // name
            // 
            name.HeaderText = "name";
            name.MinimumWidth = 20;
            name.Name = "name";
            name.ReadOnly = true;
            // 
            // pubkey
            // 
            pubkey.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            pubkey.HeaderText = "pubkey";
            pubkey.MinimumWidth = 20;
            pubkey.Name = "pubkey";
            pubkey.ReadOnly = true;
            pubkey.Width = 71;
            // 
            // nip05
            // 
            nip05.HeaderText = "nip05";
            nip05.MinimumWidth = 20;
            nip05.Name = "nip05";
            nip05.ReadOnly = true;
            nip05.Width = 110;
            // 
            // picture
            // 
            picture.HeaderText = "picture";
            picture.MinimumWidth = 20;
            picture.Name = "picture";
            picture.Width = 110;
            // 
            // created_at
            // 
            created_at.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.NullValue = null;
            created_at.DefaultCellStyle = dataGridViewCellStyle1;
            created_at.HeaderText = "created_at";
            created_at.MinimumWidth = 20;
            created_at.Name = "created_at";
            created_at.ReadOnly = true;
            created_at.Width = 86;
            // 
            // buttonSave
            // 
            buttonSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSave.Location = new Point(377, 406);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 10;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += ButtonSave_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonDelete.Location = new Point(12, 309);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(75, 23);
            buttonDelete.TabIndex = 2;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // buttonReload
            // 
            buttonReload.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonReload.Location = new Point(377, 309);
            buttonReload.Name = "buttonReload";
            buttonReload.Size = new Size(75, 23);
            buttonReload.TabIndex = 4;
            buttonReload.Text = "Reload";
            buttonReload.UseVisualStyleBackColor = true;
            buttonReload.Click += ButtonReload_Click;
            // 
            // FormManiacs
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(464, 441);
            Controls.Add(buttonReload);
            Controls.Add(buttonDelete);
            Controls.Add(buttonSave);
            Controls.Add(dataGridViewUsers);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(480, 480);
            Name = "FormManiacs";
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Users";
            FormClosing += FormManiacs_FormClosing;
            Load += FormManiacs_Load;
            KeyDown += FormManiacs_KeyDown;
            ((System.ComponentModel.ISupportInitialize)dataGridViewUsers).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridViewUsers;
        private Button buttonSave;
        private Button buttonDelete;
        private Button buttonReload;
        private DataGridViewCheckBoxColumn mute;
        private DataGridViewTextBoxColumn last_activity;
        private DataGridViewTextBoxColumn petname;
        private DataGridViewTextBoxColumn display_name;
        private DataGridViewTextBoxColumn name;
        private DataGridViewTextBoxColumn pubkey;
        private DataGridViewTextBoxColumn nip05;
        private DataGridViewTextBoxColumn picture;
        private DataGridViewTextBoxColumn created_at;
    }
}