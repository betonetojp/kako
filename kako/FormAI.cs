﻿using GenerativeAI;
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
        private ChatSessionBackUpData? _chatSessionBackUpData;

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
            // _chatSessionBackUpDataがある時はモデルを作成してIsInitializedをtrueにする
            if (_chatSessionBackUpData != null)
            {
                var apiKey = textBoxApiKey.Text;
                InitializeModel(apiKey);
                _chat = _model?.StartChat(_chatSessionBackUpData);
                IsInitialized = true;
                checkBoxInitialized.Checked = IsInitialized;
            }
        }

        private async void ButtonSummarize_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void ButtonChat_Click(object sender, EventArgs e)
        {
            try
            {
                await SendMessageAsync(textBoxChat.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
                    _chatSessionBackUpData = null;
                }
                InitializeModel(apiKey);

                var notesContent = MainForm.GetNotesContent();
                if (!IsInitialized)
                {
                    if (_chatSessionBackUpData != null)
                    {
                        // チャットセッションのバックアップデータがある場合は復元
                        _chat = _model?.StartChat(_chatSessionBackUpData);
                    }
                    else
                    {
                        // チャットセッションのバックアップデータがない場合は新規作成
                        _chat = _model?.StartChat();
                    }
                    IsInitialized = true;

                    checkBoxInitialized.Invoke((MethodInvoker)(() => checkBoxInitialized.Checked = IsInitialized));
                    var initioalPrompt = textBoxPrompt.Invoke(() => textBoxPrompt.Text);

                    if (_chat != null)
                    {
                        var result = new GenerateContentResponse();
                        try
                        {
                            result = await _chat.GenerateContentAsync(initioalPrompt);
                            SaveAISettings();
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        finally
                        {
                            DisplayResult(result.Text());
                        }
                    }
                }

                notesContent = textBoxPromptForEveryMessage.Invoke(() => textBoxPromptForEveryMessage.Text)
                             + notesContent;

                if (_chat != null)
                {
                    var result = new GenerateContentResponse();
                    try
                    {
                        result = await _chat.GenerateContentAsync(notesContent);

                        var history = _chat.History;
                        // historyを最初の2件は保持して最新の設定ターン数をその後に追加
                        if (history != null && history.Count > 2 + (int)numericUpDownTurns.Value * 2)
                        {
                            var firstTwo = history.Take(2).ToList(); // 最初の2件を保持
                            var latestTen = history.Skip(history.Count - (int)numericUpDownTurns.Value * 2).ToList();
                            history = firstTwo.Concat(latestTen).ToList();
                            _chat.History = history;
                        }

                        SaveAISettings();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        DisplayResult(result.Text());
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
                _chatSessionBackUpData = null;
            }
            InitializeModel(apiKey);

            if (!IsInitialized)
            {
                if (_chatSessionBackUpData != null)
                {
                    // チャットセッションのバックアップデータがある場合は復元
                    _chat = _model?.StartChat(_chatSessionBackUpData);
                }
                else
                {
                    // チャットセッションのバックアップデータがない場合は新規作成
                    _chat = _model?.StartChat();
                }
            }

            bool success = false;
            if (_chat != null)
            {
                var result = new GenerateContentResponse();
                try
                {
                    result = await _chat.GenerateContentAsync(message);

                    var history = _chat.History;
                    // historyを最初の2件は保持して最新の設定ターン数をその後に追加
                    if (history != null && history.Count > 2 + (int)numericUpDownTurns.Value * 2)
                    {
                        var firstTwo = history.Take(2).ToList(); // 最初の2件を保持
                        var latestTen = history.Skip(history.Count - (int)numericUpDownTurns.Value * 2).ToList();
                        history = firstTwo.Concat(latestTen).ToList();
                        _chat.History = history;
                    }

                    SaveAISettings();
                    success = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    DisplayResult(result.Text());
                    textBoxChat.Text = string.Empty;
                    textBoxChat.Focus();
                }
            }
            return success;
        }

        private void InitializeModel(string apiKey)
        {
            try
            {
                _model ??= new GenerativeModel(apiKey, textBoxModel.Text);
                _model.UseGoogleSearch = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (MainForm != null)
                {
                    MainForm.LastCreatedAt = DateTimeOffset.MinValue;
                    MainForm.LatestCreatedAt = DateTimeOffset.MinValue;
                }
            }
        }

        private void DisplayResult(string? result)
        {
            if (result == null)
            {
                textBoxAnswer.Invoke((MethodInvoker)(() => textBoxAnswer.Text = "＊ 通信異常が発生しました ＊"));
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
            try
            {
                var settings = new AISettings
                {
                    NumberOfPosts = (int)numericUpDownNumberOfPosts.Value,
                    Turns = (int)numericUpDownTurns.Value,
                    Model = textBoxModel.Text,
                    Prompt = textBoxPrompt.Text,
                    PromptForEveryMessage = textBoxPromptForEveryMessage.Text,
                    PromptForReply = textBoxPromptForReply.Text
                };
                Tools.SaveAISettings(settings);

                // チャットセッションのバックアップデータがある場合は保存
                if (_chat != null)
                {
                    _chatSessionBackUpData = _chat.CreateChatSessionBackUpData();

                    // csをJSON形式で保存
                    Tools.SaveChatSession(_chatSessionBackUpData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void LoadAISettings()
        {
            try
            {
                var settings = Tools.LoadAISettings();
                if (settings.NumberOfPosts < 1)
                {
                    settings.NumberOfPosts = 1000;
                }
                if (settings.Turns < 1)
                {
                    settings.Turns = 30;
                }
                numericUpDownNumberOfPosts.Value = settings.NumberOfPosts;
                numericUpDownTurns.Value = settings.Turns;
                textBoxModel.Text = settings.Model;
                textBoxPrompt.Text = settings.Prompt;
                textBoxPromptForEveryMessage.Text = settings.PromptForEveryMessage;
                textBoxPromptForReply.Text = settings.PromptForReply;

                // チャットセッションの復元
                _chatSessionBackUpData = Tools.LoadChatSession();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
            //if (!string.IsNullOrEmpty(textBoxApiKey.Text))
            //{
            //    _ = SummarizeNotesAsync();
            //}
            // モーダル解除
            //Close();
            Hide();
        }
    }
}
