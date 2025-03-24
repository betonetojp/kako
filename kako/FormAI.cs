using GenerativeAI.Methods;
using GenerativeAI.Models;
using GenerativeAI.Types;
using System.Diagnostics;

namespace kako
{
    public partial class FormAI : Form
    {
        internal FormMain? MainForm { get; set; }
        private const string ApiKeyTarget = "kako_ApiKey";
        private GenerativeModel? _model;
        private ChatSession? _chat;
        internal bool IsInitialized = false;

        public FormAI()
        {
            InitializeComponent();
            LoadApiKey();
            LoadAISettings();
            // textBoxModelが空の時はデフォルト値を設定
            if (string.IsNullOrEmpty(textBoxModel.Text))
            {
                textBoxModel.Text = "gemini-2.0-flash";
            }
        }

        private async void ButtonSummarize_Click(object sender, EventArgs e)
        {
            if (!IsInitialized)
            {
                if (MainForm != null)
                {
                    MainForm.LastCreatedAt = DateTimeOffset.MinValue;
                    MainForm.LatestCreatedAt = DateTimeOffset.MinValue;
                }
            }
            await SummarizeNotesAsync();
        }

        private async void ButtonChat_Click(object sender, EventArgs e)
        {
            await SendMessageAsync(textBoxChat.Text);
        }

        internal async Task<bool> SummarizeNotesAsync()
        {
            textBoxAnswer.Invoke((MethodInvoker)(() => textBoxAnswer.Text = string.Empty));

            var apiKey = textBoxApiKey.Text;

            bool success = false;
            if (MainForm != null)
            {
                if (!IsInitialized)
                {
                    _model = null;
                }
                InitializeModel(apiKey);

                var notesContent = MainForm.GetNotesContent();
                if (!IsInitialized)
                {
                    _chat = _model?.StartChat(new StartChatParams());
                    IsInitialized = true;
                    checkBoxInitialized.Invoke((MethodInvoker)(() => checkBoxInitialized.Checked = IsInitialized));
                    notesContent = textBoxPrompt.Invoke(() => textBoxPrompt.Text)
                                 + textBoxPromptForEveryMessage.Invoke(() => textBoxPromptForEveryMessage.Text)
                                 + notesContent;
                }
                else
                {
                    notesContent = textBoxPromptForEveryMessage.Invoke(() => textBoxPromptForEveryMessage.Text)
                                 + notesContent;
                }

                if (_chat != null)
                {
                    string? result = null;
                    try
                    {
                        result = await _chat.SendMessageAsync(notesContent);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        DisplayResult(result);
                    }
                }
            }
            return success;
        }
        internal async Task<bool> SendMessageAsync(string message)
        {
            textBoxAnswer.Invoke((MethodInvoker)(() => textBoxAnswer.Text = string.Empty));

            var apiKey = textBoxApiKey.Invoke(() => textBoxApiKey.Text);

            if (!IsInitialized)
            {
                _model = null;
            }
            InitializeModel(apiKey);

            if (!IsInitialized)
            {
                _chat = _model?.StartChat(new StartChatParams());
            }

            bool success = false;
            if (_chat != null)
            {
                string? result = null;
                try
                {
                    result = await _chat.SendMessageAsync(message);
                    success = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    DisplayResult(result);
                    textBoxChat.Text = string.Empty;
                    textBoxChat.Focus();
                }
            }
            return success;
        }

        private void InitializeModel(string apiKey)
        {
            _model ??= new GenerativeModel(apiKey, textBoxModel.Text);
        }

        private void DisplayResult(string? result)
        {
            if (result == null)
            {
                textBoxAnswer.Invoke((MethodInvoker)(() => textBoxAnswer.Text = "電波が悪いみたいです。"));
                //IsInitialized = false;
                //checkBoxInitialized.Invoke((MethodInvoker)(() => checkBoxInitialized.Checked = IsInitialized));
                if (MainForm != null)
                {
                    MainForm.LastCreatedAt = DateTimeOffset.MinValue;
                    MainForm.LatestCreatedAt = DateTimeOffset.MinValue;
                }

            }
            else
            {
                textBoxAnswer.Invoke((MethodInvoker)(() => textBoxAnswer.Text = result.Replace("\n", "\r\n")));
            }
        }

        private void TextBoxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // エンターキーを無効化
                ButtonChat_Click(sender, e);
            }
        }

        private static void SaveApiKey(string apiKey)
        {
            Tools.SaveApiKey(ApiKeyTarget + "_" + Tools.LoadPubkey(), apiKey);
        }

        private void LoadApiKey()
        {
            try
            {
                var apiKey = Tools.LoadApiKey(ApiKeyTarget + "_" + Tools.LoadPubkey());
                if (!string.IsNullOrEmpty(apiKey))
                {
                    textBoxApiKey.Text = apiKey;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SaveAISettings()
        {
            var settings = new AISettings
            {
                NumberOfPosts = (int)numericUpDownNumberOfPosts.Value,
                Model = textBoxModel.Text,
                Prompt = textBoxPrompt.Text,
                PromptForEveryMessage = textBoxPromptForEveryMessage.Text,
                PromptForReply = textBoxPromptForReply.Text
            };
            Tools.SaveAISettings(settings);
        }

        private void LoadAISettings()
        {
            var settings = Tools.LoadAISettings();
            numericUpDownNumberOfPosts.Value = settings.NumberOfPosts;
            textBoxModel.Text = settings.Model;
            textBoxPrompt.Text = settings.Prompt;
            textBoxPromptForEveryMessage.Text = settings.PromptForEveryMessage;
            textBoxPromptForReply.Text = settings.PromptForReply;
        }

        private void CheckBoxInitialized_CheckedChanged(object sender, EventArgs e)
        {
            IsInitialized = checkBoxInitialized.Checked;
        }

        private void LinkLabelGetApiKey_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelGetApiKey.LinkVisited = true;
            var app = new ProcessStartInfo
            {
                FileName = "https://aistudio.google.com/apikey",
                UseShellExecute = true
            };
            Process.Start(app);
        }

        private void FormAI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            SaveApiKey(textBoxApiKey.Text);
            SaveAISettings();
            Hide();
        }

        private void FormAI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                //Close();
                SaveApiKey(textBoxApiKey.Text);
                SaveAISettings();
                Hide();
            }
        }

        private void FormAI_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxApiKey.Text))
            {
                _ = SummarizeNotesAsync();
            }
            // モーダル解除
            //Close();
            Hide();
        }
    }
}
