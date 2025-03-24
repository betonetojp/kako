using kako.Properties;
using NNostr.Client;
using NNostr.Client.Protocols;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace kako
{
    public partial class FormMain : Form
    {
        #region フィールド
        private const int HOTKEY_ID = 1;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int WM_HOTKEY = 0x0312;

        private const string NostrPattern = @"nostr:(\w+)";
        private const string ImagePattern = @"(https?:\/\/.*\.(jpg|jpeg|png|gif|bmp|webp))";
        private const string UrlPattern = @"(https?:\/\/[^\s]+)";

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly string _configPath = Path.Combine(Application.StartupPath, "kako.config");

        private readonly FormSetting _formSetting = new();
        private FormManiacs _formManiacs = new();
        private FormRelayList _formRelayList = new();
        private FormAI _formAI = new();

        private string _nsec = string.Empty;
        private string _npubHex = string.Empty;

        /// <summary>
        /// フォロイー公開鍵のハッシュセット
        /// </summary>
        private readonly HashSet<string> _followeesHexs = [];
        /// <summary>
        /// ユーザー辞書
        /// </summary>
        internal Dictionary<string, User?> Users = [];

        private bool _minimizeToTray;
        private bool _addClient;

        private string _director = string.Empty;
        private bool _showOnlyFollowees;
        private bool _usePetname;
        private bool _mentionEveryHour;
        private int _mentionMinutes;
        private bool _mentionMode;
        private List<string> _forceCommands = [];
        private List<string> _callCommands = [];
        private bool _openMode;
        private int _callReplyLimit;

        private double _tempOpacity = 1.00;

        // 重複イベントIDを保存するリスト
        private readonly LinkedList<string> _displayedEventIds = new();

        //private readonly LinkedList<NostrEvent> _noteEvents = new();

        private List<Client> _clients = [];

        private System.Threading.Timer? _dailyTimer;
        private bool _reallyClose = false;
        private static Mutex? _mutex;

        // 前回の最新created_at
        internal DateTimeOffset LastCreatedAt = DateTimeOffset.MinValue;
        // 最新のcreated_at
        internal DateTimeOffset LatestCreatedAt = DateTimeOffset.MinValue;

        // スタミナ管理
        private int _callReplyCount = 0;
        private bool _alreadyPostedBreakMessage = false;
        #endregion

        #region コンストラクタ
        // コンストラクタ
        public FormMain()
        {
            InitializeComponent();

            // アプリケーションの実行パスを取得
            string exePath = Application.ExecutablePath;
            string mutexName = $"kakoMutex_{exePath.Replace("\\", "_")}";

            // 二重起動を防ぐためのミューテックスを作成
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                // 既に起動している場合はメッセージを表示して終了
                MessageBox.Show("Already running.", "kako", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            // ボタンの画像をDPIに合わせて表示
            float scale = CreateGraphics().DpiX / 96f;
            int size = (int)(16 * scale);
            if (scale < 2.0f)
            {
                buttonRelayList.Image = new Bitmap(Resources.icons8_list_16, size, size);
                buttonStart.Image = new Bitmap(Resources.icons8_start_16, size, size);
                buttonStop.Image = new Bitmap(Resources.icons8_stop_16, size, size);
                buttonSetting.Image = new Bitmap(Resources.icons8_setting_16, size, size);
            }
            else
            {
                buttonRelayList.Image = new Bitmap(Resources.icons8_list_32, size, size);
                buttonStart.Image = new Bitmap(Resources.icons8_start_32, size, size);
                buttonStop.Image = new Bitmap(Resources.icons8_stop_32, size, size);
                buttonSetting.Image = new Bitmap(Resources.icons8_setting_32, size, size);
            }

            Setting.Load(_configPath);
            Users = Tools.LoadUsers();
            _clients = Tools.LoadClients();

            Location = Setting.Location;
            if (new Point(0, 0) == Location || Location.X < 0 || Location.Y < 0)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
            Size = Setting.Size;
            TopMost = Setting.TopMost;
            Opacity = Setting.Opacity;
            if (0 == Opacity)
            {
                Opacity = 1;
            }
            _tempOpacity = Opacity;
            _minimizeToTray = Setting.MinimizeToTray;
            notifyIcon.Visible = _minimizeToTray;
            _addClient = Setting.AddClient;

            _director = Setting.Director;
            _showOnlyFollowees = Setting.ShowOnlyFollowees;
            _usePetname = Setting.UsePetname;
            _mentionEveryHour = Setting.MentionEveryHour;
            _mentionMinutes = Setting.MentionMinutes;
            _mentionMode = Setting.MentionMode;
            _forceCommands = Setting.ForceCommands;
            _callCommands = Setting.CallCommands;
            _openMode = Setting.OpenMode;
            _callReplyLimit = Setting.CallReplyLimit;

            dataGridViewNotes.Columns["name"].Width = Setting.NameColumnWidth;
            dataGridViewNotes.GridColor = Tools.HexToColor(Setting.GridColor);
            dataGridViewNotes.DefaultCellStyle.SelectionBackColor = Tools.HexToColor(Setting.GridColor);

            _formManiacs.MainForm = this;
            _formAI.MainForm = this;

            // タイマーの初期化
            SetDailyTimer();
        }
        #endregion

        #region Startボタン
        // Startボタン
        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            try
            {
                int connectCount;
                if (NostrAccess.Clients != null)
                {
                    connectCount = await NostrAccess.ConnectAsync();
                }
                else
                {
                    connectCount = await NostrAccess.ConnectAsync();

                    if (NostrAccess.Clients != null)
                    {
                        NostrAccess.Clients.EventsReceived += OnClientOnUsersInfoEventsReceived;
                        NostrAccess.Clients.EventsReceived += OnClientOnTimeLineEventsReceived;
                    }
                }

                toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.RelayStatusList));

                switch (connectCount)
                {
                    case 0:
                        labelRelays.Text = "No relay enabled.";
                        buttonStart.Enabled = false;
                        return;
                    case 1:
                        labelRelays.Text = $"{connectCount} relay";
                        break;
                    default:
                        labelRelays.Text = $"{connectCount} relays";
                        break;
                }

                await NostrAccess.SubscribeAsync();

                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                dataGridViewNotes.Focus();

                // ログイン済みの時
                //if (!string.IsNullOrEmpty(_director))
                {
                    // フォロイーを購読をする
                    await NostrAccess.SubscribeFollowsAsync(_director.ConvertToHex());

                    // ログインユーザー名取得
                    var loginName = GetName(_npubHex);
                    var directorName = GetName(_director.ConvertToHex());
                    if (!string.IsNullOrEmpty(loginName))
                    {
                        Text = $"kako - @{loginName} to {directorName}";
                        notifyIcon.Text = $"kako - @{loginName} to {directorName}";
                    }
                }

                dataGridViewNotes.Rows.Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Could not start.";
            }
        }
        #endregion

        #region ユーザー情報イベント受信時処理
        /// <summary>
        /// ユーザー情報イベント受信時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnClientOnUsersInfoEventsReceived(object? sender, (string subscriptionId, NostrEvent[] events) args)
        {
            if (args.subscriptionId == NostrAccess.GetFolloweesSubscriptionId)
            {
                #region フォロイー購読
                foreach (var nostrEvent in args.events)
                {
                    // フォローリスト
                    if (3 == nostrEvent.Kind)
                    {
                        var tags = nostrEvent.Tags;
                        foreach (var tag in tags)
                        {
                            if ("p" == tag.TagIdentifier)
                            {
                                // 公開鍵をハッシュに保存
                                _followeesHexs.Add(tag.Data[0]);

                                // プロフィール購読
                                await NostrAccess.SubscribeProfilesAsync([tag.Data[0]]);

                                // petnameをユーザー辞書に保存
                                if (2 < tag.Data.Count)
                                {
                                    Users.TryGetValue(tag.Data[0], out User? user);
                                    if (user != null)
                                    {
                                        user.PetName = tag.Data[2];
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            else if (args.subscriptionId == NostrAccess.GetProfilesSubscriptionId)
            {
                #region プロフィール購読
                foreach (var nostrEvent in args.events)
                {
                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    // プロフィール
                    if (0 == nostrEvent.Kind && nostrEvent.Content != null && nostrEvent.PublicKey != null)
                    {
                        var newUserData = Tools.JsonToUser(nostrEvent.Content, nostrEvent.CreatedAt);
                        if (newUserData != null)
                        {
                            DateTimeOffset? cratedAt = DateTimeOffset.MinValue;
                            if (Users.TryGetValue(nostrEvent.PublicKey, out User? existingUserData))
                            {
                                cratedAt = existingUserData?.CreatedAt;
                            }
                            if (false == existingUserData?.Mute)
                            {
                                // 既にミュートオフのMostrアカウントのミュートを解除
                                newUserData.Mute = false;
                            }
                            if (cratedAt == null || (cratedAt < newUserData.CreatedAt))
                            {
                                newUserData.LastActivity = DateTime.Now;
                                newUserData.PetName = existingUserData?.PetName;
                                Tools.SaveUsers(Users);
                                // 辞書に追加（上書き）
                                Users[nostrEvent.PublicKey] = newUserData;
                                Debug.WriteLine($"cratedAt updated {cratedAt} -> {newUserData.CreatedAt}");
                                Debug.WriteLine($"プロフィール更新: {newUserData.DisplayName} @{newUserData.Name}");
                            }
                        }
                    }
                }
                #endregion
            }
        }
        #endregion

        #region タイムラインイベント受信時処理
        /// <summary>
        /// タイムラインイベント受信時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param="args"></param>
        private async void OnClientOnTimeLineEventsReceived(object? sender, (string subscriptionId, NostrEvent[] events) args)
        {
            if (args.subscriptionId == NostrAccess.SubscriptionId)
            {
                #region タイムライン購読
                foreach (var nostrEvent in args.events)
                {
                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    var content = nostrEvent.Content;
                    if (content != null)
                    {
                        string userName = string.Empty;
                        User? user = null;

                        // フォロイーチェック
                        string headMark = "-";
                        if (_followeesHexs.Contains(nostrEvent.PublicKey))
                        {
                            headMark = "*";
                        }

                        #region テキストノート
                        if (1 == nostrEvent.Kind)
                        {
                            string editedContent = content;

                            // nostr:npub1またはnostr:nprofile1が含まれている場合、@ユーザー名を取得
                            MatchCollection matches = Regex.Matches(editedContent, @"nostr:(npub1\w+|nprofile1\w+)");
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    string npubOrNprofile = match.Groups[1].Value.ConvertToHex();
                                    // ユーザー名取得
                                    string mentionedUserName = $"［👤{GetUserName(npubOrNprofile)}］";
                                    // nostr:npub1またはnostr:nprofile1を@ユーザー名に置き換え
                                    editedContent = editedContent.Replace(match.Value, mentionedUserName);
                                }
                            }

                            //string nostrPattern = @"nostr:(\w+)";
                            // nostr:を含む場合、(citations omitted)に置き換え
                            editedContent = Regex.Replace(editedContent, NostrPattern, "［🗒️］");

                            //string imagePattern = @"(https?:\/\/.*\.(jpg|jpeg|png|gif|bmp|webp))";
                            // 画像URLを含む場合、(image)に置き換え
                            editedContent = Regex.Replace(editedContent, ImagePattern, "［🖼️］", RegexOptions.IgnoreCase);

                            //string urlPattern = @"(https?:\/\/[^\s]+)";
                            // URLを含む場合、(url)に置き換え
                            editedContent = Regex.Replace(editedContent, UrlPattern, "［🔗］", RegexOptions.IgnoreCase);

                            // フォロイー限定表示オンでフォロイーじゃない時は表示しない
                            if (_showOnlyFollowees && !_followeesHexs.Contains(nostrEvent.PublicKey))
                            {
                                continue;
                            }
                            // ミュートしている時は表示しない
                            if (IsMuted(nostrEvent.PublicKey))
                            {
                                continue;
                            }
                            // pタグにミュートされている公開鍵が含まれている時は表示しない
                            if (nostrEvent.GetTaggedPublicKeys().Any(pk => IsMuted(pk)))
                            {
                                continue;
                            }
                            // 自分の投稿は表示しない
                            if (_npubHex == nostrEvent.PublicKey)
                            {
                                continue;
                            }

                            string whoToNotify = string.Empty;

                            // オーナーコマンド
                            try
                            {
                                whoToNotify = _director.ConvertToHex();
                                if (nostrEvent.PublicKey == whoToNotify)
                                {
                                    // 返信の時
                                    var replyTags = nostrEvent.GetTaggedData("p");
                                    if (replyTags != null && 0 < replyTags.Length)
                                    {
                                        // 返信先の公開鍵を取得
                                        string replyTo = replyTags[0];
                                        // 返信先が自分の時
                                        if (replyTo.Equals(_npubHex))
                                        {
                                            // リセットコマンド
                                            if (content == "reset")
                                            {
                                                await PostAsync("AIをリセットしました。", nostrEvent);
                                                _formAI.checkBoxInitialized.Checked = false;
                                                continue;
                                            }
                                            // スタートコマンド
                                            if (content == "start")
                                            {
                                                await PostAsync("定期投稿を有効にしました。", nostrEvent);
                                                _mentionEveryHour = true;
                                                continue;
                                            }
                                            // ストップコマンド
                                            if (content == "stop")
                                            {
                                                await PostAsync("定期投稿を無効にしました。", nostrEvent);
                                                _mentionEveryHour = false;
                                                continue;
                                            }
                                        }
                                    }

                                    // まとめコマンド
                                    if (_forceCommands.Contains(content))
                                    {
                                        if (!_formAI.IsInitialized)
                                        {
                                            LastCreatedAt = DateTimeOffset.MinValue;
                                            LatestCreatedAt = DateTimeOffset.MinValue;
                                        }
                                        bool success = await _formAI.SummarizeNotesAsync();
                                        // 1秒待つ
                                        await Task.Delay(1000);
                                        await PostAsync(_formAI.textBoxAnswer.Text, nostrEvent);
                                        if (success)
                                        {
                                            dataGridViewNotes.Rows.Clear();
                                            GC.Collect();
                                            GC.WaitForPendingFinalizers();
                                            continue;
                                        }
                                    }
                                }

                                if (_openMode || nostrEvent.PublicKey == whoToNotify)
                                {
                                    // 呼出コマンド
                                    if (_callCommands.Contains(content))
                                    {
                                        if (_alreadyPostedBreakMessage)
                                        {
                                            Debug.WriteLine("スタミナが切れています。");
                                        }
                                        else
                                        {
                                            await _formAI.SendMessageAsync(GetUserName(nostrEvent.PublicKey) + "さんが呼んでいます。返事をしてください。");
                                            // 1秒待つ
                                            await Task.Delay(1000);
                                            await PostAsync(_formAI.textBoxAnswer.Text, nostrEvent);
                                            _callReplyCount++;

                                            if (_callReplyCount >= _callReplyLimit)
                                            {
                                                await _formAI.SendMessageAsync("疲れたからしばらく休むことを宣言ください。");
                                                // 1秒待つ
                                                await Task.Delay(1000);
                                                if (_openMode)
                                                {
                                                    await PostAsync(_formAI.textBoxAnswer.Text);
                                                }
                                                else
                                                {
                                                    await PostAsync(_formAI.textBoxAnswer.Text, nostrEvent);
                                                }
                                                _alreadyPostedBreakMessage = true;
                                                Debug.WriteLine("スタミナが切れました。");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 返信の時
                                        var replyTags = nostrEvent.GetTaggedData("p");
                                        if (replyTags != null && 0 < replyTags.Length)
                                        {
                                            // 返信先の公開鍵を取得
                                            string replyTo = replyTags[0];
                                            // 返信先が自分の時
                                            if (replyTo.Equals(_npubHex))
                                            {
                                                if (_alreadyPostedBreakMessage)
                                                {
                                                    Debug.WriteLine("スタミナが切れています。");
                                                }
                                                else
                                                {
                                                    string promptForReply = _formAI.textBoxPromptForReply.Text;
                                                    await _formAI.SendMessageAsync(promptForReply + "\r\n" + GetUserName(nostrEvent.PublicKey) + "さんからの返信：\r\n" + content);
                                                    // 1秒待つ
                                                    await Task.Delay(1000);
                                                    await PostAsync(_formAI.textBoxAnswer.Text, nostrEvent);
                                                    _callReplyCount++;

                                                    if (_callReplyCount >= _callReplyLimit)
                                                    {
                                                        await _formAI.SendMessageAsync("疲れたからしばらく休むことを宣言ください。");
                                                        // 1秒待つ
                                                        await Task.Delay(1000);
                                                        if (_openMode)
                                                        {
                                                            await PostAsync(_formAI.textBoxAnswer.Text);
                                                        }
                                                        else
                                                        {
                                                            await PostAsync(_formAI.textBoxAnswer.Text, nostrEvent);
                                                        }
                                                        _alreadyPostedBreakMessage = true;
                                                        Debug.WriteLine("スタミナが切れました。");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"通知先変換失敗: {ex.Message}");
                                continue;
                            }

                            // プロフィール購読
                            await NostrAccess.SubscribeProfilesAsync([nostrEvent.PublicKey]);

                            // ユーザー取得
                            user = await GetUserAsync(nostrEvent.PublicKey);
                            // ユーザーが見つからない時は表示しない
                            if (user == null)
                            {
                                continue;
                            }
                            // ユーザー表示名取得
                            userName = GetUserName(nostrEvent.PublicKey);

                            bool isReply = false;
                            var e = nostrEvent.GetTaggedData("e");
                            var p = nostrEvent.GetTaggedData("p");
                            var q = nostrEvent.GetTaggedData("q");
                            if (e != null && 0 < e.Length ||
                                p != null && 0 < p.Length ||
                                q != null && 0 < q.Length)
                            {
                                isReply = true;
                                //headMark = "<";

                                if (p != null && 0 < p.Length)
                                {
                                    string mentionedUserNames = string.Empty;
                                    foreach (var u in p)
                                    {
                                        mentionedUserNames = $"{mentionedUserNames} {GetUserName(u)}";
                                    }
                                    editedContent = $"［💬{mentionedUserNames}］\r\n{editedContent}";
                                }
                            }

                            // グリッドに表示
                            //_noteEvents.AddFirst(nostrEvent);
                            DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
                            dataGridViewNotes.Rows.Insert(
                                0,
                                dto.ToLocalTime(),
                                new Bitmap(1, 1),
                                $"{headMark} {userName}",
                                //nostrEvent.Content,
                                editedContent,
                                nostrEvent.Id,
                                nostrEvent.PublicKey,
                                nostrEvent.Kind
                                );
                            //dataGridViewNotes.Sort(dataGridViewNotes.Columns["time"], ListSortDirection.Descending);

                            // リプライの時は背景色変更
                            if (isReply)
                            {
                                dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReplyColor);
                            }

                            // 行を装飾
                            EditRow(nostrEvent, userName);

                            // 改行をスペースに置き換えてログ表示
                            Debug.WriteLine($"{userName}: {content.Replace('\n', ' ')}");
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        #endregion

        #region グリッド行装飾
        private void EditRow(NostrEvent nostrEvent, string userName)
        {
            // avatar列のToolTipに表示名を設定
            dataGridViewNotes.Rows[0].Cells["avatar"].ToolTipText = userName;
            // note列のToolTipにcontentを設定
            dataGridViewNotes.Rows[0].Cells["note"].ToolTipText = nostrEvent.Content;

            // avastar列の背景色をpubkeyColorに変更
            var pubkeyColor = Tools.HexToColor(nostrEvent.PublicKey[..6]); // [i..j] で「i番目からj番目の範囲」
            dataGridViewNotes.Rows[0].Cells["avatar"].Style.BackColor = pubkeyColor;

            // クライアントタグによる背景色変更
            var userClient = nostrEvent.GetTaggedData("client");
            if (userClient != null && 0 < userClient.Length)
            {
                Color clientColor = Color.WhiteSmoke;

                // userClient[0]を_clientsから検索して色を取得
                var client = _clients.FirstOrDefault(c => c.Name == userClient[0]);
                if (client != null && client.ColorCode != null)
                {
                    clientColor = Tools.HexToColor(client.ColorCode);
                }
                // time列の背景色をclientColorに変更
                dataGridViewNotes.Rows[0].Cells["time"].Style.BackColor = clientColor;
            }

            // content-warning
            string[]? reason = null;
            try
            {
                reason = nostrEvent.GetTaggedData("content-warning"); // reasonが無いと例外吐く
            }
            catch
            {
                reason = [""];
            }
            if (reason != null && 0 < reason.Length)
            {
                dataGridViewNotes.Rows[0].Cells["note"].Value = "CW: " + reason[0];
                //// ツールチップにcontentを設定
                //dataGridViewNotes.Rows[0].Cells["note"].ToolTipText = nostrEvent.Content;
                // note列の背景色をCWColorに変更
                dataGridViewNotes.Rows[0].Cells["note"].Style.BackColor = Tools.HexToColor(Setting.CWColor);
            }
        }
        #endregion

        #region ユーザー取得
        private async Task<User?> GetUserAsync(string pubkey)
        {
            User? user = null;
            int retryCount = 0;
            while (retryCount < 10)
            {
                Debug.WriteLine($"retryCount = {retryCount} {GetUserName(pubkey)}");
                Users.TryGetValue(pubkey, out user);
                // ユーザーが見つかった場合、ループを抜ける
                if (user != null)
                {
                    break;
                }
                // 一定時間待機してから再試行
                await Task.Delay(100);
                retryCount++;
            }
            return user;
        }
        #endregion

        #region Stopボタン
        // Stopボタン
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }

            try
            {
                NostrAccess.CloseSubscriptions();
                labelRelays.Text = "Close subscription.";

                _ = NostrAccess.Clients.Disconnect();
                labelRelays.Text = "Disconnect.";
                NostrAccess.Clients.Dispose();
                NostrAccess.Clients = null;

                buttonStart.Enabled = true;
                buttonStart.Focus();
                buttonStop.Enabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Could not stop.";
            }
        }
        #endregion

        #region 投稿処理
        /// <summary>
        /// 投稿処理
        /// </summary>
        /// <returns></returns>
        private async Task PostAsync(string content, NostrEvent? rootEvent = null, bool isQuote = false)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            if (rootEvent != null)
            {
                if (isQuote)
                {
                    tags.Add(new NostrEventTag() { TagIdentifier = "q", Data = [rootEvent.Id, string.Empty] });
                }
                else
                {
                    tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootEvent.Id, string.Empty] });
                    tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [rootEvent.PublicKey] });
                }
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["kako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 1,
                Content = content.Replace("\r\n", "\n"),
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }

        private async Task MentionAsync(string content)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [_director.ConvertToHex()] });

            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["kako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 1,
                Content = "nostr:" + _director + " " + content.Replace("\r\n", "\n"),
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region リアクション処理
        private async Task ReactionAsync(string e, string p, int k, string? content, string? url = null)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [e] });
            tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [p] });
            tags.Add(new NostrEventTag() { TagIdentifier = "k", Data = [k.ToString()] });
            if (!string.IsNullOrEmpty(url))
            {
                tags.Add(new NostrEventTag() { TagIdentifier = "emoji", Data = [$"{content}", $"{url}"] });
                content = $":{content}:";
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["kako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 7,
                Content = content,
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region リポスト処理
        private async Task RepostAsync(string e, string p, int k)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [e, string.Empty] });
            tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [p] });
            if (1 != k)
            {
                tags.Add(new NostrEventTag() { TagIdentifier = "k", Data = [k.ToString()] });
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["kako"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = k == 1 ? 6 : 16,
                Content = string.Empty,
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region Settingボタン
        // Settingボタン
        private async void ButtonSetting_Click(object sender, EventArgs e)
        {
            // 開く前
            _formSetting.checkBoxTopMost.Checked = TopMost;
            Opacity = _tempOpacity;
            _formSetting.trackBarOpacity.Value = (int)(Opacity * 100);
            _formSetting.checkBoxMinimizeToTray.Checked = _minimizeToTray;
            _formSetting.checkBoxAddClient.Checked = _addClient;

            _formSetting.textBoxDirector.Text = _director;
            _formSetting.checkBoxShowOnlyFollowees.Checked = _showOnlyFollowees;
            _formSetting.checkBoxUsePetname.Checked = _usePetname;
            _formSetting.checkBoxMentionEveryHour.Checked = _mentionEveryHour;
            _formSetting.numericUpDownMentionMinutes.Value = _mentionMinutes;
            _formSetting.checkBoxMentionMode.Checked = _mentionMode;
            _formSetting.textBoxForceCommands.Text = string.Join("\r\n", _forceCommands);
            _formSetting.textBoxCallCommands.Text = string.Join("\r\n", _callCommands);
            _formSetting.checkBoxOpenMode.Checked = _openMode;
            _formSetting.numericUpDownCallReplyLimit.Value = _callReplyLimit;

            _formSetting.textBoxNsec.Text = _nsec;
            _formSetting.textBoxNpub.Text = _nsec.GetNpub();

            // 開く
            _formSetting.ShowDialog(this);

            // 閉じた後
            TopMost = _formSetting.checkBoxTopMost.Checked;
            Opacity = _formSetting.trackBarOpacity.Value / 100.0;
            _tempOpacity = Opacity;
            _minimizeToTray = _formSetting.checkBoxMinimizeToTray.Checked;
            notifyIcon.Visible = _minimizeToTray;
            _addClient = _formSetting.checkBoxAddClient.Checked;

            _director = _formSetting.textBoxDirector.Text;
            _showOnlyFollowees = _formSetting.checkBoxShowOnlyFollowees.Checked;
            _usePetname = _formSetting.checkBoxUsePetname.Checked;
            _mentionEveryHour = _formSetting.checkBoxMentionEveryHour.Checked;
            _mentionMinutes = (int)_formSetting.numericUpDownMentionMinutes.Value;
            _mentionMode = _formSetting.checkBoxMentionMode.Checked;
            _forceCommands = [.. _formSetting.textBoxForceCommands.Text.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)];
            _callCommands = [.. _formSetting.textBoxCallCommands.Text.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)];
            _openMode = _formSetting.checkBoxOpenMode.Checked;
            _callReplyLimit = (int)_formSetting.numericUpDownCallReplyLimit.Value;

            _nsec = _formSetting.textBoxNsec.Text;

            // タイマーの初期化
            SetDailyTimer();
            // スタミナリセット
            _callReplyCount = 0;
            _alreadyPostedBreakMessage = false;

            try
            {
                // 別アカウントログイン失敗に備えてクリアしておく
                _npubHex = string.Empty;
                _followeesHexs.Clear();
                Text = "kako";
                notifyIcon.Text = "kako";

                // 秘密鍵と公開鍵取得
                _npubHex = _nsec.GetNpubHex();

                // ログイン済みの時
                //if (!string.IsNullOrEmpty(_director))
                {
                    int connectCount = await NostrAccess.ConnectAsync();

                    toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.RelayStatusList));

                    switch (connectCount)
                    {
                        case 0:
                            labelRelays.Text = "No relay enabled.";
                            return;
                        case 1:
                            labelRelays.Text = $"{connectCount} relay";
                            break;
                        default:
                            labelRelays.Text = $"{connectCount} relays";
                            break;
                    }

                    // フォロイーを購読をする
                    await NostrAccess.SubscribeFollowsAsync(_director.ConvertToHex());

                    // ログインユーザー名取得
                    var loginName = GetName(_npubHex);
                    var directorName = GetName(_director.ConvertToHex());
                    if (!string.IsNullOrEmpty(loginName))
                    {
                        Text = $"kako - @{loginName} to {directorName}";
                        notifyIcon.Text = $"kako - @{loginName} to {directorName}";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
            // nsecを保存
            Tools.SavePubkey(_npubHex);
            SaveNsec(_npubHex, _nsec);

            Setting.TopMost = TopMost;
            Setting.Opacity = Opacity;
            Setting.MinimizeToTray = _minimizeToTray;
            Setting.AddClient = _addClient;

            Setting.Director = _director;
            Setting.ShowOnlyFollowees = _showOnlyFollowees;
            Setting.UsePetname = _usePetname;
            Setting.MentionEveryHour = _mentionEveryHour;
            Setting.MentionMinutes = _mentionMinutes;
            Setting.MentionMode = _mentionMode;
            Setting.ForceCommands = _forceCommands;
            Setting.CallCommands = _callCommands;
            Setting.OpenMode = _openMode;
            Setting.CallReplyLimit = _callReplyLimit;

            Setting.Save(_configPath);
            _clients = Tools.LoadClients();

            dataGridViewNotes.Focus();
        }
        #endregion

        #region 複数リレーからの処理済みイベントを除外
        /// <summary>
        /// 複数リレーからの処理済みイベントを除外
        /// </summary>
        /// <param name="eventId"></param>
        private bool RemoveCompletedEventIds(string eventId)
        {
            if (_displayedEventIds.Contains(eventId))
            {
                return true;
            }
            if (_displayedEventIds.Count >= 4096)
            {
                _displayedEventIds.RemoveFirst();
            }
            _displayedEventIds.AddLast(eventId);
            return false;
        }
        #endregion

        #region 透明解除処理
        // マウス入った時
        private void Control_MouseEnter(object sender, EventArgs e)
        {
            _tempOpacity = Opacity;
            Opacity = 1.00;
        }

        // マウス出た時
        private void Control_MouseLeave(object sender, EventArgs e)
        {
            Opacity = _tempOpacity;
        }
        #endregion

        #region ユーザー名を取得する
        /// <summary>
        /// ユーザー名を取得する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ユーザー名</returns>
        private string? GetName(string publicKeyHex)
        {
            // 情報があればユーザー名を取得
            Users.TryGetValue(publicKeyHex, out User? user);
            string? userName = string.Empty;
            if (user != null)
            {
                userName = user.Name;
                // 取得日更新
                user.LastActivity = DateTime.Now;
                Tools.SaveUsers(Users);
            }
            return userName;
        }
        #endregion

        #region ユーザー表示名を取得する
        /// <summary>
        /// ユーザー表示名を取得する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ユーザー表示名</returns>
        private string GetUserName(string publicKeyHex)
        {
            // 情報があれば表示名を取得
            Users.TryGetValue(publicKeyHex, out User? user);
            string? userName = "???";
            if (user != null)
            {
                userName = user.DisplayName;
                // display_nameが無い場合は@nameとする
                if (userName == null || string.Empty == userName)
                {
                    //userName = $"@{user.Name}";
                    userName = $"{user.Name}";
                }
                // petnameがある場合は📛petnameとする
                if (_usePetname && !string.IsNullOrEmpty(user.PetName))
                {
                    //userName = $"📛{user.PetName}";
                    userName = $"{user.PetName}";
                }
                // 取得日更新
                user.LastActivity = DateTime.Now;
                Tools.SaveUsers(Users);
                //Debug.WriteLine($"名前取得: {user.DisplayName} @{user.Name} 📛{user.PetName}");
            }
            return userName;
        }
        #endregion

        #region ミュートされているか確認する
        /// <summary>
        /// ミュートされているか確認する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ミュートフラグ</returns>
        private bool IsMuted(string publicKeyHex)
        {
            if (Users.TryGetValue(publicKeyHex, out User? user))
            {
                if (user != null)
                {
                    return user.Mute;
                }
            }
            return false;
        }
        #endregion

        #region 閉じる
        // 閉じる
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_minimizeToTray && !_reallyClose && e.CloseReason == CloseReason.UserClosing)
            {
                // 閉じるボタンが押されたときは最小化
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Hide(); // フォームを非表示にします（タスクトレイに格納）
            }
            else
            {
                // ホットキーの登録を解除
                UnregisterHotKey(this.Handle, HOTKEY_ID);

                NostrAccess.CloseSubscriptions();
                NostrAccess.DisconnectAndDispose();

                if (FormWindowState.Normal != WindowState)
                {
                    // 最小化最大化状態の時、元の位置と大きさを保存
                    Setting.Location = RestoreBounds.Location;
                    Setting.Size = RestoreBounds.Size;
                }
                else
                {
                    Setting.Location = Location;
                    Setting.Size = Size;
                }
                Setting.NameColumnWidth = dataGridViewNotes.Columns["name"].Width;
                Setting.Save(_configPath);
                Tools.SaveUsers(Users);

                _dailyTimer?.Change(Timeout.Infinite, 0);
                _dailyTimer?.Dispose();

                Application.Exit();
            }
        }
        #endregion

        #region ロード時
        // ロード時
        private void FormMain_Load(object sender, EventArgs e)
        {
            // Ctrl + Shift + A をホットキーとして登録
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, (int)Keys.A);

            //_formAI.ShowDialog();
            _formAI.Show(this);
            //_formAI.Hide();

            try
            {
                _npubHex = Tools.LoadPubkey();
                _nsec = LoadNsec();
                _formSetting.textBoxNsec.Text = _nsec;
                _formSetting.textBoxNpub.Text = _nsec.GetNpub();
                if (!string.IsNullOrEmpty(_formSetting.textBoxNpub.Text))
                {
                    _formSetting.textBoxNsec.Enabled = false;
                }

                ButtonStart_Click(sender, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region 画面表示切替
        // 画面表示切替
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            // F2キーでtime列の表示切替
            if (e.KeyCode == Keys.F2)
            {
                dataGridViewNotes.Columns["time"].Visible = !dataGridViewNotes.Columns["time"].Visible;
            }
            // F3キーでavatar列の表示切替
            if (e.KeyCode == Keys.F3)
            {
                dataGridViewNotes.Columns["avatar"].Visible = !dataGridViewNotes.Columns["avatar"].Visible;
            }
            // F4キーでname列の表示切替
            if (e.KeyCode == Keys.F4)
            {
                dataGridViewNotes.Columns["name"].Visible = !dataGridViewNotes.Columns["name"].Visible;
            }
            // F5キーでFormAIを表示
            if (e.KeyCode == Keys.F5)
            {
                if (_formAI == null || _formAI.IsDisposed)
                {
                    _formAI = new FormAI();
                    _formAI.MainForm = this;
                }
                if (!_formAI.Visible)
                {
                    _formAI.Show(this);
                }
            }

            if (e.KeyCode == Keys.Escape)
            {
                ButtonSetting_Click(sender, e);
            }

            if (e.KeyCode == Keys.F10)
            {
                var ev = new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0);
                FormMain_MouseClick(sender, ev);
            }

            if (e.KeyCode == Keys.F9)
            {
                var ev = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0);
                FormMain_MouseDoubleClick(sender, ev);
            }
        }
        #endregion

        #region マニアクス表示
        private void FormMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (_formManiacs == null || _formManiacs.IsDisposed)
                {
                    _formManiacs = new FormManiacs
                    {
                        MainForm = this
                    };
                }
                if (!_formManiacs.Visible)
                {
                    _formManiacs.Show(this);
                }
            }
        }
        #endregion

        #region リレーリスト表示
        private void ButtonRelayList_Click(object sender, EventArgs e)
        {
            _formRelayList = new FormRelayList();
            if (_formRelayList.ShowDialog(this) == DialogResult.OK)
            {
                ButtonStop_Click(sender, e);
                ButtonStart_Click(sender, e);
            }
            _formRelayList.Dispose();
            dataGridViewNotes.Focus();
        }
        #endregion

        #region グリッドキー入力
        private void DataGridViewNotes_KeyDown(object sender, KeyEventArgs e)
        {
            // Wキーで選択行を上に
            if (e.KeyCode == Keys.W)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index > 0)
                {
                    dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index - 1].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.SelectedRows[0].Index];
                }
            }
            // Sキーで選択行を下に
            if (e.KeyCode == Keys.S)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index < dataGridViewNotes.Rows.Count - 1)
                {
                    dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index + 1].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.SelectedRows[0].Index];
                }
            }
            // Shift + Wキーで選択行を最上部に
            if (e.KeyCode == Keys.W && e.Shift)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index > 0)
                {
                    dataGridViewNotes.Rows[0].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", 0];
                }
            }
            // Shift + Sキーで選択行を最下部に
            if (e.KeyCode == Keys.S && e.Shift)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index < dataGridViewNotes.Rows.Count - 1)
                {
                    dataGridViewNotes.Rows[^1].Selected = true; // インデックス演算子 [^i] で「後ろからi番目の要素」
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.Rows.Count - 1];
                }
            }
            // Zキーでnote列の折り返し切り替え
            if (e.KeyCode == Keys.Z)
            {
                var ev = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0);
                FormMain_MouseDoubleClick(sender, ev);
            }
        }
        #endregion

        #region フォームマウスダブルクリック
        private void FormMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode != DataGridViewTriState.True)
            {
                dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            else
            {
                dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode = DataGridViewTriState.NotSet;
            }
        }
        #endregion

        #region フォーム最初の表示時
        private void FormMain_Shown(object sender, EventArgs e)
        {
            dataGridViewNotes.Focus();
        }
        #endregion

        #region 秘密鍵管理
        private static void SaveNsec(string pubkey, string nsec)
        {
            // 前回のトークンを削除
            DeletePreviousTarget();

            // 新しいtargetを生成して保存
            string target = Guid.NewGuid().ToString();
            Tools.SavePassword("kako_" + target, pubkey, nsec);
            SaveTarget(target);
        }

        private static void SaveTarget(string target)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("target");
            config.AppSettings.Settings.Add("target", target);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void DeletePreviousTarget()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var previousTarget = config.AppSettings.Settings["target"]?.Value;
            if (!string.IsNullOrEmpty(previousTarget))
            {
                Tools.DeletePassword("kako_" + previousTarget);
                config.AppSettings.Settings.Remove("target");
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private static string LoadNsec()
        {
            string target = LoadTarget();
            if (!string.IsNullOrEmpty(target))
            {
                return Tools.LoadPassword("kako_" + target);
            }
            return string.Empty;
        }

        private static string LoadTarget()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings["target"]?.Value ?? string.Empty;
        }
        #endregion

        #region グローバルホットキー
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                this.Activate(); // FormMainをアクティブにする
            }
            base.WndProc(ref m);
        }
        #endregion

        #region タスクトレイ最小化
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            // 右クリック時は抜ける
            if (e is MouseEventArgs me && me.Button == MouseButtons.Right)
            {
                return;
            }

            // 最小化時は通常表示に戻す
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 設定画面がすでに開かれている場合は抜ける
            if (_formSetting.Visible)
            {
                return;
            }
            Show();
            WindowState = FormWindowState.Normal;
            ButtonSetting_Click(sender, e);
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _reallyClose = true;
            Close();
        }

        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            // 最小化時はタスクトレイに格納
            if (_minimizeToTray && WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }
        #endregion

        #region ノート取得
        public string GetNotesContent()
        {
            var notes = new StringBuilder();
            int count = 0;
            try
            {
                Debug.Print($"_lastCreatedAt: {LastCreatedAt}");
                foreach (DataGridViewRow row in dataGridViewNotes.Rows)
                {
                    // timeが_lastCreatedAtの時は抜ける
                    if (DateTimeOffset.TryParse(row.Cells["time"].Value?.ToString(), out DateTimeOffset createdAt) && createdAt == LastCreatedAt)
                    {
                        Debug.Print($"_lastCreatedAt: {LastCreatedAt}");
                        break;
                    }
                    // 一番上の行のtimeをDateTimeOffsetに変換して_latestCreatedAtに保存
                    if (count == 0)
                    {
                        if (DateTimeOffset.TryParse(row.Cells["time"].Value?.ToString(), out DateTimeOffset latestCreatedAt))
                        {
                            LatestCreatedAt = latestCreatedAt;
                        }
                    }
                    // 指定件数で抜ける
                    if (count >= _formAI.numericUpDownNumberOfPosts.Value)
                    {
                        break;
                    }
                    // kindが7の時はスキップ
                    if ((int)row.Cells["kind"].Value == 7)
                    {
                        continue;
                    }
                    notes.Append(row.Cells["time"].Value?.ToString() + "\r\n");
                    notes.Append(row.Cells["name"].Value?.ToString()?.Substring(2) + "\r\n");
                    notes.Append(row.Cells["note"].Value?.ToString() + "\r\n\r\n");
                    notes.AppendLine();
                    count++;
                }
                LastCreatedAt = LatestCreatedAt;
                Debug.Print($"_latestCreatedAt: {LatestCreatedAt} count: {count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return notes.ToString();
        }
        #endregion

        #region デイリータイマー
        // デイリータイマーの設定を毎時に変更
        private void SetDailyTimer()
        {
            var now = DateTime.Now;
            var nextTrigger = new DateTime(now.Year, now.Month, now.Day, now.Hour, _mentionMinutes, 0);
            if (now.Minute >= _mentionMinutes)
            {
                nextTrigger = nextTrigger.AddHours(1);
            }
            TimeSpan timeToGo = nextTrigger - now;
            _dailyTimer?.Dispose();
            _dailyTimer = new System.Threading.Timer(DailyTimerCallback, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        // デイリータイマーのコールバック
        private async void DailyTimerCallback(object? state)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }

            try
            {
                labelRelays.Invoke((MethodInvoker)(() => labelRelays.Text = "Reconnecting..."));

                await NostrAccess.Clients.Disconnect();
                await NostrAccess.ConnectAsync();
                await NostrAccess.SubscribeAsync();

                // ログイン済みの時
                if (!string.IsNullOrEmpty(_npubHex))
                {
                    // フォロイーを購読する
                    await NostrAccess.SubscribeFollowsAsync(_npubHex);
                }

                labelRelays.Invoke((MethodInvoker)(() => labelRelays.Text = "Reconnected successfully."));

                // 定時まとめメンション
                if (_mentionEveryHour)
                {
                    if (!_formAI.IsInitialized)
                    {
                        LastCreatedAt = DateTimeOffset.MinValue;
                        LatestCreatedAt = DateTimeOffset.MinValue;
                    }
                    bool success = await _formAI.SummarizeNotesAsync();
                    // 1秒待つ
                    await Task.Delay(1000);
                    string answerText = string.Empty;
                    Invoke((MethodInvoker)(() => answerText = _formAI.textBoxAnswer.Text));
                    if (string.IsNullOrEmpty(_director) || !_mentionMode)
                    {
                        await PostAsync(answerText);
                    }
                    else
                    {
                        await MentionAsync(answerText);
                    }
                    if (success)
                    {
                        dataGridViewNotes.Invoke((MethodInvoker)(() => dataGridViewNotes.Rows.Clear()));
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }

                // 投稿後に labelRelays.Text と toolTipRelays を元に戻す
                labelRelays.Invoke((MethodInvoker)(() =>
                {
                    int relayCount = NostrAccess.Relays.Length;

                    toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.RelayStatusList));

                    switch (relayCount)
                    {
                        case 0:
                            labelRelays.Text = "No relay enabled.";
                            break;
                        case 1:
                            labelRelays.Text = $"{NostrAccess.Relays.Length} relay";
                            break;
                        default:
                            labelRelays.Text = $"{NostrAccess.Relays.Length} relays";
                            break;
                    }
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Invoke((MethodInvoker)(() => labelRelays.Text = "Reconnection failed."));
                MessageBox.Show($"再接続に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // スタミナリセット
            _callReplyCount = 0;
            _alreadyPostedBreakMessage = false;

            // タイマーの再設定
            SetDailyTimer();
        }
        #endregion
    }
}
