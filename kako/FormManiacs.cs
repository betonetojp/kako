﻿using System.ComponentModel;

namespace kako
{
    public partial class FormManiacs : Form
    {
        internal FormMain? MainForm { get; set; }
        public FormManiacs()
        {
            InitializeComponent();
        }

        private void FormManiacs_Load(object sender, EventArgs e)
        {
            if (MainForm != null)
            {
                dataGridViewUsers.Rows.Clear();
                foreach (var user in MainForm.Users)
                {
                    dataGridViewUsers.Rows.Add(
                        user.Value?.Mute,
                        user.Value?.LastActivity,
                        user.Value?.PetName,
                        user.Value?.DisplayName,
                        user.Value?.Name,
                        user.Key,
                        user.Value?.Nip05,
                        user.Value?.Picture,
                        user.Value?.CreatedAt
                        );
                }
                dataGridViewUsers.Sort(dataGridViewUsers.Columns["last_activity"], ListSortDirection.Descending);
                dataGridViewUsers.ClearSelection();
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (MainForm != null)
            {
                var users = new Dictionary<string, User?>();
                foreach (DataGridViewRow row in dataGridViewUsers.Rows)
                {
                    var pubkey = (string)row.Cells["pubkey"].Value;
                    if (pubkey != null)
                    {
                        var user = new User
                        {
                            Mute = (bool)(row.Cells["mute"].Value ?? false),
                            LastActivity = (DateTime?)row.Cells["last_activity"].Value ?? null,
                            PetName = (string)row.Cells["petname"].Value,
                            DisplayName = (string)row.Cells["display_name"].Value,
                            Name = (string)row.Cells["name"].Value,
                            Nip05 = (string)row.Cells["nip05"].Value,
                            Picture = (string)row.Cells["picture"].Value,
                            CreatedAt = (DateTimeOffset?)row.Cells["created_at"].Value ?? null
                        };
                        users.Add(pubkey, user);
                    }
                }
                MainForm.Users = users;
            }
            Close();
        }

        private void FormManiacs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MainForm != null)
            {
                Tools.SaveUsers(MainForm.Users);
                MainForm.Users = Tools.LoadUsers();
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewUsers.SelectedRows)
            {
                dataGridViewUsers.Rows.Remove(row);
            }
        }

        private void ButtonReload_Click(object sender, EventArgs e)
        {
            FormManiacs_Load(sender, e);
        }

        private void FormManiacs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F10)
            {
                Close();
            }
        }
    }
}
