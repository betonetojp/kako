using kako.Properties;
using System.Diagnostics;

namespace kako
{
    public partial class FormSetting : Form
    {
        public FormSetting()
        {
            InitializeComponent();

            // ボタンの画像をDPIに合わせて表示
            using var graphics = CreateGraphics();
            float scale = graphics.DpiX / 96f;
            int size = (int)(16 * scale);
            if (scale < 2.0f)
            {
                buttonLogOut.Image = new Bitmap(Resources.icons8_log_out_16, size, size);
            }
            else
            {
                buttonLogOut.Image = new Bitmap(Resources.icons8_log_out_32, size, size);
            }
            // モード選択アイテムを追加
            comboBoxMode.Items.AddRange(["Note (Kind 1)", "Channel (Kind 42)", "BitChat (Kind 20000)"]);
            toolTipLogOut.SetToolTip(checkBoxReactToZaps, "Bot宛のZapを受け取ったとき、AIでお礼を返します。プロフィールにLightning Addressが必要です。");
        }

        private void FormSetting_Load(object sender, EventArgs e)
        {
            labelOpacity.Text = $"{trackBarOpacity.Value}%";
            UpdateModeUI();
        }

        private void ComboBoxMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateModeUI();
        }

        private void UpdateModeUI()
        {
            var isChannel = (comboBoxMode.SelectedIndex == (int)BotMode.Channel);
            var isBitChat = (comboBoxMode.SelectedIndex == (int)BotMode.BitChat);

            labelChannelId.Visible = isChannel;
            textBoxChannelId.Visible = isChannel;
            textBoxChannelId.Enabled = isChannel;

            labelGeohash.Visible = isBitChat;
            textBoxGeohash.Visible = isBitChat;
            textBoxGeohash.Enabled = isBitChat;

            labelBotName.Visible = isBitChat;
            textBoxBotName.Visible = isBitChat;
            textBoxBotName.Enabled = isBitChat;
        }

        private void FormSetting_Shown(object sender, EventArgs e)
        {
            checkBoxTopMost.Focus();
        }

        private void FormSetting_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void TrackBarOpacity_Scroll(object sender, EventArgs e)
        {
            labelOpacity.Text = $"{trackBarOpacity.Value}%";
            if (Owner != null)
            {
                Owner.Opacity = trackBarOpacity.Value / 100.0;
            }
        }

        private void LinkLabelIcons8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelIcons8.LinkVisited = true;
            var app = new ProcessStartInfo
            {
                FileName = "https://icons8.com",
                UseShellExecute = true
            };
            Process.Start(app);
        }

        private void LinkLabelVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelVersion.LinkVisited = true;
            var app = new ProcessStartInfo
            {
                FileName = "https://github.com/betonetojp/kako",
                UseShellExecute = true
            };
            Process.Start(app);
        }

        private void TextBoxNsec_Leave(object sender, EventArgs e)
        {
            textBoxNpub.Text = textBoxNsec.Text.GetNpub();
            if (!string.IsNullOrEmpty(textBoxNpub.Text))
            {
                textBoxNsec.Enabled = false;
            }
        }

        private void ButtonLogOut_Click(object sender, EventArgs e)
        {
            textBoxNsec.Enabled = true;
            textBoxNsec.Text = string.Empty;
            textBoxNpub.Text = string.Empty;
        }
    }
}
