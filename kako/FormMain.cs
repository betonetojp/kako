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
        private const string ImagePattern = @"(https?:\/\/[^\s]*\.(jpg|jpeg|png|gif|bmp|webp))";
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

        private BotMode _mode = BotMode.Note;
        private string _channelId = string.Empty;
        private string _botName = "まとめbot";
        private string _geohash = "xn";

        private string _director = string.Empty;
        private bool _showOnlyFollowees;
        private bool _usePetname;
        private bool _summarizeEveryHour;
        private int _summarizeMinutes;
        private bool _mentionMode;
        private bool _addNostrNpub1;
        private bool _summarizeByEventCount;
        private int _eventThreshold;
        private List<string> _forceCommands = [];
        private List<string> _callCommands = [];
        private bool _openMode;
        private bool _reactToZaps = true;
        private int _callReplyLimit;
        private bool _appendUserId = true;

        private double _tempOpacity = 1.00;

        private string GetAppTitle()
        {
            return _mode switch
            {
                BotMode.Channel => "kakochannel",
                BotMode.BitChat => "kakochat",
                _ => "kako"
            };
        }

        // 重複イベントIDを保存するリスト
        private readonly LinkedList<string> _displayedEventIds = new();

        // プロフィール取得中の公開鍵
        private readonly HashSet<string> _fetchingProfileHexs = [];

        // 受信イベントのリレー追跡
        private readonly Dictionary<string, List<string>> _eventSeenOn = [];

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

        // まとめ中
        private bool _isSummarizing = false;
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
            using var graphics = CreateGraphics();
            float scale = graphics.DpiX / 96f;
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

            _mode = Setting.Mode;
            _channelId = Setting.ChannelId;
            _botName = Setting.BotName;
            _geohash = Setting.Geohash;
            _director = Setting.Director;
            _showOnlyFollowees = Setting.ShowOnlyFollowees;
            _usePetname = Setting.UsePetname;
            _summarizeEveryHour = Setting.SummarizeEveryHour;
            _summarizeMinutes = Setting.SummarizeMinutes;
            _mentionMode = Setting.MentionMode;
            _addNostrNpub1 = Setting.AddNostrNpub1;
            _summarizeByEventCount = Setting.SummarizeByEventCount;
            _eventThreshold = Setting.EventThreshold;
            _forceCommands = Setting.ForceCommands;
            _callCommands = Setting.CallCommands;
            _openMode = Setting.OpenMode;
            _reactToZaps = Setting.ReactToZaps;
            _callReplyLimit = Setting.CallReplyLimit;
            _appendUserId = Setting.AppendUserId;

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
                int connectCount = await NostrAccess.ConnectAsync();

                if (NostrAccess.Clients != null)
                {
                    NostrAccess.Clients.EventsReceived -= OnClientOnUsersInfoEventsReceived;
                    NostrAccess.Clients.EventsReceived -= OnClientOnTimeLineEventsReceived;
                    NostrAccess.Clients.EventsReceived += OnClientOnUsersInfoEventsReceived;
                    NostrAccess.Clients.EventsReceived += OnClientOnTimeLineEventsReceived;
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

                await NostrAccess.SubscribeAsync(_mode, _channelId.ConvertEventIdToHex(), _npubHex);

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
                        var appTitle = GetAppTitle();
                        Text = $"{appTitle} - @{loginName} to {directorName}";
                        notifyIcon.Text = $"{appTitle} - @{loginName} to {directorName}";
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
                            lock (Users)
                            {
                                DateTimeOffset? createdAt = DateTimeOffset.MinValue;
                                if (Users.TryGetValue(nostrEvent.PublicKey, out User? existingUserData))
                                {
                                    createdAt = existingUserData?.CreatedAt;
                                }
                                if (false == existingUserData?.Mute)
                                {
                                    // 既にミュートオフのMostrアカウントのミュートを解除
                                    newUserData.Mute = false;
                                }
                                if (createdAt == null || (createdAt < newUserData.CreatedAt))
                                {
                                    newUserData.LastActivity = DateTime.Now;
                                    newUserData.PetName = existingUserData?.PetName;
                                    // 辞書に追加（上書き）
                                    Users[nostrEvent.PublicKey] = newUserData;
                                    Debug.WriteLine($"createdAt updated {createdAt} -> {newUserData.CreatedAt}");
                                    Debug.WriteLine($"プロフィール更新: {newUserData.DisplayName} @{newUserData.Name}");
                                }
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
                    var relay = (sender as NostrClient)?.Relay?.ToString();
                    if (!string.IsNullOrEmpty(relay))
                    {
                        lock (_eventSeenOn)
                        {
                            if (!_eventSeenOn.TryGetValue(nostrEvent.Id, out var relays))
                            {
                                relays = [];
                                _eventSeenOn[nostrEvent.Id] = relays;
                            }
                            if (!relays.Contains(relay))
                            {
                                relays.Add(relay);
                            }
                        }
                    }

                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    if (nostrEvent.Kind == 9735)
                    {
                        await HandleIncomingZapAsync(nostrEvent);
                        continue;
                    }

                    //var content = nostrEvent.Content;
                    // 500文字以上は切り捨て
                    var content = nostrEvent.Content?.Substring(0, Math.Min(500, nostrEvent.Content.Length));
                    if (content != null)
                    {
                        string userName = string.Empty;

                        // フォロイーチェック
                        string headMark = "-";
                        if (_followeesHexs.Contains(nostrEvent.PublicKey))
                        {
                            headMark = "*";
                        }

                        #region モード別タイムラインイベント処理
                        int targetKind = _mode switch
                        {
                            BotMode.Channel => 42,
                            BotMode.BitChat => 20000,
                            _ => 1
                        };

                        if (nostrEvent.Kind == targetKind)
                        {
                            if (_mode == BotMode.Channel)
                            {
                                var targetChannelHex = _channelId.ConvertEventIdToHex();
                                if (!string.IsNullOrEmpty(targetChannelHex))
                                {
                                    var eTags = nostrEvent.GetTaggedData("e");
                                    if (eTags == null || !eTags.Contains(targetChannelHex, StringComparer.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }
                                }
                            }
                            else if (_mode == BotMode.BitChat)
                            {
                                var targetGeohash = string.IsNullOrWhiteSpace(_geohash) ? "xn" : _geohash;
                                var g = nostrEvent.GetTaggedData("g");
                                if (g == null || g.Length == 0 || g[0] != targetGeohash)
                                {
                                    continue;
                                }
                            }

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

                            // 先にタイムラインへ表示してからコマンド／返信に反応する
                            await NostrAccess.SubscribeProfilesAsync([nostrEvent.PublicKey]);
                            FetchProfileIfNeeded(nostrEvent.PublicKey);

                            if (_mode == BotMode.BitChat)
                            {
                                var n = nostrEvent.GetTaggedData("n");
                                if (n != null && 0 < n.Length && !string.IsNullOrEmpty(n[0]))
                                {
                                    userName = n[0];
                                }
                            }
                            if (string.IsNullOrEmpty(userName))
                            {
                                userName = GetUserName(nostrEvent.PublicKey);
                                if (userName == "???" && nostrEvent.PublicKey.Length >= 8)
                                {
                                    userName = nostrEvent.PublicKey[..8];
                                }
                            }

                            bool isReply = false;
                            var e = nostrEvent.GetTaggedData("e");
                            var p = nostrEvent.GetTaggedData("p");
                            var q = nostrEvent.GetTaggedData("q");
                            if (e != null && 0 < e.Length ||
                                p != null && 0 < p.Length ||
                                q != null && 0 < q.Length)
                            {
                                isReply = true;

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

                            DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
                            dataGridViewNotes.Rows.Insert(
                                0,
                                dto.ToLocalTime(),
                                new Bitmap(1, 1),
                                $"{headMark} {userName}",
                                editedContent,
                                nostrEvent.Id,
                                nostrEvent.PublicKey,
                                nostrEvent.Kind
                                );
                            dataGridViewNotes.Rows[0].Tag = isReply;

                            if (isReply)
                            {
                                dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReplyColor);
                            }

                            EditRow(nostrEvent, userName);
                            Debug.WriteLine($"{userName}: {content.Replace('\n', ' ')}");

                            string whoToNotify = string.Empty;

                            // オーナーコマンド・呼出・返信
                            try
                            {
                                whoToNotify = _director.ConvertToHex();
                                if (nostrEvent.PublicKey == whoToNotify)
                                {
                                    // 返信の時
                                    var replyTags = nostrEvent.GetTaggedData("p");
                                    if (replyTags != null && 0 < replyTags.Length)
                                    {
                                        // 返信先の公開鍵を取得（投稿者自身を除外した先頭）
                                        string? replyTo = replyTags.FirstOrDefault(pk => pk != nostrEvent.PublicKey);
                                        // 返信先が自分の時
                                        if (replyTo != null && replyTo.Equals(_npubHex))
                                        {
                                            // リセットコマンド
                                            if (content == "reset")
                                            {
                                                await PostAsync("＊ AIをリセットしました ＊", nostrEvent);
                                                _formAI.checkBoxInitialized.Checked = false;
                                                await SummarizeAndPostAsync();
                                                // スタミナリセット
                                                _callReplyCount = 0;
                                                _alreadyPostedBreakMessage = false;
                                                continue;
                                            }
                                            // スタートコマンド
                                            if (content == "start")
                                            {
                                                await PostAsync("＊ 定期投稿を有効にしました ＊", nostrEvent);
                                                _summarizeEveryHour = true;
                                                continue;
                                            }
                                            if (content == "start2")
                                            {
                                                await PostAsync("＊ 投稿を有効にしました ＊", nostrEvent);
                                                _summarizeByEventCount = true;
                                                continue;
                                            }
                                            // ストップコマンド
                                            if (content == "stop")
                                            {
                                                await PostAsync("＊ 投稿を無効にしました ＊", nostrEvent);
                                                _summarizeEveryHour = false;
                                                _summarizeByEventCount = false;
                                                continue;
                                            }

                                            // オープンコマンド
                                            if (content == "open")
                                            {
                                                _openMode = true;
                                                await PostAsync("＊ 返信応答を有効にしました ＊", nostrEvent);
                                                // スタミナリセット
                                                _callReplyCount = 0;
                                                _alreadyPostedBreakMessage = false;
                                                continue;
                                            }

                                            // クローズコマンド
                                            if (content == "close")
                                            {
                                                _openMode = false;
                                                await PostAsync("＊ 返信応答を無効にしました ＊", nostrEvent);
                                                continue;
                                            }

                                            // クリアコマンド
                                            if (content == "clear")
                                            {
                                                await PostAsync("＊ イベントをクリアしました ＊", nostrEvent);
                                                dataGridViewNotes.Rows.Clear();
                                                GC.Collect();
                                                GC.WaitForPendingFinalizers();
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
                                        bool success = await _formAI.SummarizeNotesAsync(true);
                                        // 1秒待つ
                                        await Task.Delay(1000);
                                        var answer = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n');
                                        if (!string.IsNullOrWhiteSpace(answer))
                                        {
                                            await PostAsync(answer, nostrEvent);
                                        }
                                        if (success)
                                        {
                                            dataGridViewNotes.Rows.Clear();
                                            GC.Collect();
                                            GC.WaitForPendingFinalizers();
                                        }
                                        continue;
                                    }
                                }

                                if (_openMode || nostrEvent.PublicKey == whoToNotify)
                                {
                                    // 呼出コマンド（前方一致判定）
                                    var matchedCmd = _callCommands.FirstOrDefault(cmd => content.StartsWith(cmd));
                                    if (matchedCmd != null)
                                    {
                                        if (_alreadyPostedBreakMessage)
                                        {
                                            Debug.WriteLine("スタミナが切れています。");
                                        }
                                        else
                                        {
                                            var authorWithId = GetAuthorNameWithId(nostrEvent.PublicKey);
                                            bool success = await _formAI.SendMessageAsync(authorWithId + "さんからの返信：\r\n" + content);
                                            // 1秒待つ
                                            await Task.Delay(1000);
                                            var answer = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n');
                                            if (!string.IsNullOrWhiteSpace(answer))
                                            {
                                                await PostAsync(answer, nostrEvent);
                                                if (success)
                                                {
                                                    _callReplyCount++;
                                                }
                                            }

                                            if (success && _callReplyCount >= _callReplyLimit)
                                            {
                                                bool breakSuccess = await _formAI.SendMessageAsync("疲れたからしばらく休むことを宣言ください。");
                                                if (breakSuccess)
                                                {
                                                    // 1秒待つ
                                                    await Task.Delay(1000);
                                                    var breakAnswer = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n');
                                                    if (!string.IsNullOrWhiteSpace(breakAnswer))
                                                    {
                                                        if (_mode == BotMode.Channel)
                                                        {
                                                            await PostAsync(breakAnswer, nostrEvent);
                                                        }
                                                        else if (_openMode)
                                                        {
                                                            await PostAsync(breakAnswer);
                                                        }
                                                        else
                                                        {
                                                            await PostAsync(breakAnswer, nostrEvent);
                                                        }
                                                    }
                                                }
                                                _alreadyPostedBreakMessage = true;
                                                Debug.WriteLine("スタミナが切れました。");
                                            }
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        // 返信の時（pタグが4個以上の多人数巻き込み・ヘルスレッドは除外）
                                        var replyTags = nostrEvent.GetTaggedData("p");
                                        if (replyTags != null && 0 < replyTags.Length && replyTags.Length <= 3)
                                        {
                                            // 返信先の公開鍵を取得（投稿者自身を除外した先頭）
                                            string? replyTo = replyTags.FirstOrDefault(pk => pk != nostrEvent.PublicKey);
                                            // 返信先が自分の時
                                            if (replyTo != null && replyTo.Equals(_npubHex))
                                            {
                                                if (_alreadyPostedBreakMessage)
                                                {
                                                    Debug.WriteLine("スタミナが切れています。");
                                                }
                                                else
                                                {
                                                    string promptForReply = _formAI.textBoxPromptForReply.Text;
                                                    var authorWithId = GetAuthorNameWithId(nostrEvent.PublicKey);
                                                    bool success = await _formAI.SendMessageAsync(promptForReply + "\r\n" + authorWithId + "さんからの返信：\r\n" + content);
                                                    // 1秒待つ
                                                    await Task.Delay(1000);
                                                    var answer = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n');
                                                    if (!string.IsNullOrWhiteSpace(answer))
                                                    {
                                                        await PostAsync(answer, nostrEvent);
                                                        if (success)
                                                        {
                                                            _callReplyCount++;
                                                        }
                                                    }

                                                    if (success && _callReplyCount >= _callReplyLimit)
                                                    {
                                                        bool breakSuccess = await _formAI.SendMessageAsync("疲れたからしばらく休むことを宣言ください。");
                                                        if (breakSuccess)
                                                        {
                                                            // 1秒待つ
                                                            await Task.Delay(1000);
                                                            var breakAnswer = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n');
                                                            if (!string.IsNullOrWhiteSpace(breakAnswer))
                                                            {
                                                                if (_mode == BotMode.Channel)
                                                                {
                                                                    await PostAsync(breakAnswer, nostrEvent);
                                                                }
                                                                else if (_openMode)
                                                                {
                                                                    await PostAsync(breakAnswer);
                                                                }
                                                                else
                                                                {
                                                                    await PostAsync(breakAnswer, nostrEvent);
                                                                }
                                                            }
                                                        }
                                                        _alreadyPostedBreakMessage = true;
                                                        Debug.WriteLine("スタミナが切れました。");
                                                    }
                                                }
                                                continue;
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

                            // 受信投稿数によるまとめ投稿
                            if (_summarizeByEventCount && dataGridViewNotes.Rows.Count >= _eventThreshold)
                            {
                                await SummarizeAndPostAsync();
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        #endregion

        #region Zap受信
        private async Task HandleIncomingZapAsync(NostrEvent nostrEvent)
        {
            if (string.IsNullOrEmpty(_npubHex) ||
                !Tools.TryParseZapReceipt(nostrEvent, _npubHex, out var zap) ||
                string.IsNullOrEmpty(zap.SenderPubkey))
            {
                return;
            }

            if (IsMuted(zap.SenderPubkey))
            {
                return;
            }

            await NostrAccess.SubscribeProfilesAsync([zap.SenderPubkey]);
            FetchProfileIfNeeded(zap.SenderPubkey);

            string userName = GetUserName(zap.SenderPubkey);
            if (userName == "???" && zap.SenderPubkey.Length >= 8)
            {
                userName = zap.SenderPubkey[..8];
            }

            string headMark = _followeesHexs.Contains(zap.SenderPubkey) ? "*" : "-";
            string displayContent = zap.AmountSats > 0 ? $"⚡ {zap.AmountSats} sats" : "⚡ Zap";
            if (!string.IsNullOrWhiteSpace(zap.Comment))
            {
                displayContent += $"\r\n{zap.Comment}";
            }

            DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
            void AddZapRow()
            {
                dataGridViewNotes.Rows.Insert(
                    0,
                    dto.ToLocalTime(),
                    new Bitmap(1, 1),
                    $"{headMark} {userName}",
                    displayContent,
                    nostrEvent.Id,
                    zap.SenderPubkey,
                    nostrEvent.Kind
                );
                dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReactionColor);

                var displayEvent = new NostrEvent
                {
                    Id = nostrEvent.Id,
                    PublicKey = zap.SenderPubkey,
                    Content = displayContent,
                    Kind = 9735,
                    CreatedAt = nostrEvent.CreatedAt,
                    Tags = nostrEvent.Tags
                };
                EditRow(displayEvent, userName);
            }
            if (dataGridViewNotes.IsHandleCreated && dataGridViewNotes.InvokeRequired)
            {
                dataGridViewNotes.Invoke(AddZapRow);
            }
            else
            {
                AddZapRow();
            }

            Debug.WriteLine($"Zap from {userName}: {displayContent.Replace('\n', ' ')}");

            if (!_reactToZaps || zap.SenderPubkey == _npubHex)
            {
                return;
            }

            // プロフィールZap（eタグなし）や、自モード以外の投稿へのZapには反応しない
            // nokakoi 等は LNURL 互換のため Zap Request から k を落とすので、無いときはリレーから取得する
            int modeKind = _mode switch
            {
                BotMode.Channel => 42,
                BotMode.BitChat => 20000,
                _ => 1
            };
            if (string.IsNullOrEmpty(zap.TargetEventId))
            {
                Debug.WriteLine("Zap ignored for reply (profile zap / no e tag)");
                return;
            }

            int? targetKind = zap.TargetKind;
            if (targetKind == null)
            {
                targetKind = await NostrAccess.FetchEventKindAsync(zap.TargetEventId);
                Debug.WriteLine($"Zap target kind fetched: {targetKind}");
            }
            if (targetKind == null || targetKind != modeKind)
            {
                Debug.WriteLine($"Zap ignored for reply (targetEvent={zap.TargetEventId}, targetKind={targetKind}, modeKind={modeKind})");
                return;
            }

            string messageBody = string.Empty;
            var aiSettings = Tools.LoadAISettings();
            if (!_isSummarizing)
            {
                string promptForZap = string.Empty;
                _formAI.Invoke((MethodInvoker)(() => promptForZap = _formAI.textBoxPromptForZap.Text));
                if (string.IsNullOrWhiteSpace(promptForZap))
                {
                    promptForZap = aiSettings.PromptForZap;
                }
                var authorWithId = GetAuthorNameWithId(zap.SenderPubkey);
                var amountText = zap.AmountSats switch
                {
                    1 => " 1 sat の",
                    > 1 => $" {zap.AmountSats} sats の",
                    _ => string.Empty
                };
                var commentText = string.IsNullOrWhiteSpace(zap.Comment)
                    ? "コメントはありません。"
                    : $"コメント: {zap.Comment}";
                var message =
                    promptForZap + "\r\n" +
                    authorWithId + "さんから" + amountText + "Zapを受け取りました。\r\n" +
                    commentText + "\r\n" +
                    "お礼の返答をしてください。";
                bool success = await _formAI.SendMessageAsync(message);
                if (success)
                {
                    await Task.Delay(1000);
                    _formAI.Invoke((MethodInvoker)(() => messageBody = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n')));
                }
            }

            if (string.IsNullOrWhiteSpace(messageBody))
            {
                var fallbackTemplate = aiSettings.FallbackZapMessage;
                if (!string.IsNullOrWhiteSpace(fallbackTemplate))
                {
                    if (zap.AmountSats == 1)
                    {
                        messageBody = fallbackTemplate
                            .Replace("{amount}sats", "1sat")
                            .Replace("{amount} sats", "1 sat")
                            .Replace("{amount}", "1")
                            .Replace("{unit}", "sat");
                    }
                    else if (zap.AmountSats > 1)
                    {
                        messageBody = fallbackTemplate
                            .Replace("{amount}", zap.AmountSats.ToString())
                            .Replace("{unit}", "sats");
                    }
                    else
                    {
                        messageBody = fallbackTemplate
                            .Replace("{amount}sats", "")
                            .Replace("{amount} sats", "")
                            .Replace("{amount}", "")
                            .Replace("{unit}", "")
                            .Replace("  ", " ")
                            .Trim();
                    }
                }
            }

            // 本文（AI返答またはフォールバック）が空の場合は投稿しない
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                return;
            }

            // ヘッダー組み立て
            string header = string.Empty;
            var headerTemplate = aiSettings.ZapHeader;
            if (!string.IsNullOrWhiteSpace(headerTemplate))
            {
                var npub = zap.SenderPubkey.ConvertToNpub();
                var mention = !string.IsNullOrEmpty(npub) ? $"nostr:{npub}" : userName;
                header = headerTemplate
                    .Replace("{mention}", mention)
                    .Replace("{user}", userName);

                if (zap.AmountSats == 1)
                {
                    header = header
                        .Replace("{amount}sats", "1sat")
                        .Replace("{amount} sats", "1 sat")
                        .Replace("{amount}", "1")
                        .Replace("{unit}", "sat");
                }
                else if (zap.AmountSats > 1)
                {
                    header = header
                        .Replace("{amount}", zap.AmountSats.ToString())
                        .Replace("{unit}", "sats");
                }
                else
                {
                    header = header
                        .Replace("{amount}sats", "")
                        .Replace("{amount} sats", "")
                        .Replace("{amount}", "")
                        .Replace("{unit}", "")
                        .Replace("  ", " ")
                        .Trim();
                }
            }

            // コメント引用組み立て
            string quoteComment = string.Empty;
            if (!string.IsNullOrWhiteSpace(zap.Comment))
            {
                var commentLines = zap.Comment.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
                quoteComment = string.Join("\r\n", commentLines.Select(line => string.IsNullOrWhiteSpace(line) ? "💬" : $"💬 {line}"));
            }

            // ヘッダーとコメントの結合（空行なし）
            string topBlock = string.Empty;
            if (!string.IsNullOrWhiteSpace(header) && !string.IsNullOrWhiteSpace(quoteComment))
            {
                topBlock = header + "\r\n" + quoteComment;
            }
            else if (!string.IsNullOrWhiteSpace(header))
            {
                topBlock = header;
            }
            else if (!string.IsNullOrWhiteSpace(quoteComment))
            {
                topBlock = quoteComment;
            }

            // 本文との結合（空行あり）
            string answer;
            if (!string.IsNullOrWhiteSpace(topBlock) && !string.IsNullOrWhiteSpace(messageBody))
            {
                answer = topBlock + "\r\n\r\n" + messageBody;
            }
            else if (!string.IsNullOrWhiteSpace(topBlock))
            {
                answer = topBlock;
            }
            else
            {
                answer = messageBody;
            }

            var rootEvent = new NostrEvent
            {
                Id = zap.TargetEventId,
                PublicKey = zap.SenderPubkey,
                Kind = modeKind
            };

            await PostAsync(answer, rootEvent);
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
            if (!string.IsNullOrEmpty(nostrEvent.PublicKey) && nostrEvent.PublicKey.Length >= 6)
            {
                var pubkeyColor = Tools.HexToColor(nostrEvent.PublicKey[..6]); // [i..j] で「i番目からj番目の範囲」
                dataGridViewNotes.Rows[0].Cells["avatar"].Style.BackColor = pubkeyColor;
            }

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

        #region プロフィール取得（インデクサ連携）
        private void FetchProfileIfNeeded(string pubkey)
        {
            if (string.IsNullOrEmpty(pubkey)) return;

            // 既に有効な表示名がある場合は再取得不要
            if (Users.TryGetValue(pubkey, out var existingUser) && existingUser != null)
            {
                if (!string.IsNullOrEmpty(existingUser.DisplayName) || !string.IsNullOrEmpty(existingUser.Name))
                {
                    return;
                }
            }

            lock (_fetchingProfileHexs)
            {
                if (_fetchingProfileHexs.Contains(pubkey)) return;
                _fetchingProfileHexs.Add(pubkey);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var profileEvent = await NostrAccess.FetchProfileFromIndexerAsync(pubkey);
                    if (profileEvent?.Content != null)
                    {
                        var newUserData = Tools.JsonToUser(profileEvent.Content, profileEvent.CreatedAt);
                        if (newUserData != null)
                        {
                            string resolvedName = string.Empty;
                            lock (Users)
                            {
                                DateTimeOffset? createdAt = DateTimeOffset.MinValue;
                                if (Users.TryGetValue(pubkey, out User? existingUserData))
                                {
                                    createdAt = existingUserData?.CreatedAt;
                                    newUserData.PetName = existingUserData?.PetName;
                                    if (false == existingUserData?.Mute)
                                    {
                                        newUserData.Mute = false;
                                    }
                                }
                                if (createdAt == null || (createdAt < newUserData.CreatedAt))
                                {
                                    newUserData.LastActivity = DateTime.Now;
                                    Users[pubkey] = newUserData;
                                    Debug.WriteLine($"[Indexer] プロフィール取得成功: {newUserData.DisplayName} @{newUserData.Name} ({pubkey[..8]})");
                                }
                            }

                            resolvedName = GetUserName(pubkey);

                            // グリッド上の名前・ツールチップを更新
                            if (dataGridViewNotes.IsHandleCreated)
                            {
                                dataGridViewNotes.BeginInvoke(new Action(() => UpdateGridProfile(pubkey, resolvedName)));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Indexer] プロフィール取得失敗: {ex.Message}");
                }
                finally
                {
                    lock (_fetchingProfileHexs)
                    {
                        _fetchingProfileHexs.Remove(pubkey);
                    }
                }
            });
        }

        private void UpdateGridProfile(string pubkey, string name)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewNotes.Rows)
                {
                    if (row.Cells["pubkey"]?.Value?.ToString() == pubkey)
                    {
                        var currentNameCell = row.Cells["name"]?.Value?.ToString();
                        var headMark = currentNameCell?.StartsWith("*") == true ? "* " : "- ";
                        row.Cells["name"].Value = $"{headMark}{name}";
                        row.Cells["avatar"].ToolTipText = name;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateGridProfile エラー: {ex.Message}");
            }
        }
        #endregion

        #region Stopボタン
        // Stopボタン
        private async void ButtonStop_Click(object sender, EventArgs e)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }

            try
            {
                NostrAccess.CloseSubscriptions();
                labelRelays.Text = "Close subscription.";

                await NostrAccess.Clients.Disconnect();
                labelRelays.Text = "Disconnect.";
                NostrAccess.Clients.Dispose();
                NostrAccess.Clients = null;

                Tools.SaveUsers(Users);

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
        private async Task PostAsync(string content, NostrEvent? rootEvent = null, bool isQuote = false, string? extraMentionHex = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            int eventKind = _mode switch
            {
                BotMode.Channel => 42,
                BotMode.BitChat => 20000,
                _ => 1
            };

            if (_mode == BotMode.Channel)
            {
                var channelHex = _channelId.ConvertEventIdToHex();
                if (!string.IsNullOrEmpty(channelHex))
                {
                    var channelRelayHint = GetBestRelayHint(channelHex);
                    tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [channelHex, channelRelayHint, "root"] });
                }

                if (rootEvent != null)
                {
                    var targetRelayHint = GetBestRelayHint(rootEvent.Id);
                    tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootEvent.Id, targetRelayHint, "reply"] });
                    tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [rootEvent.PublicKey] });
                }
            }
            else if (_mode == BotMode.BitChat)
            {
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
                var botName = string.IsNullOrWhiteSpace(_botName) ? "まとめbot" : _botName;
                var geohash = string.IsNullOrWhiteSpace(_geohash) ? "xn" : _geohash;
                tags.Add(new NostrEventTag() { TagIdentifier = "n", Data = [botName] });
                tags.Add(new NostrEventTag() { TagIdentifier = "g", Data = [geohash] });
            }
            else
            {
                if (rootEvent != null)
                {
                    var targetRelayHint = GetBestRelayHint(rootEvent.Id);
                    if (isQuote)
                    {
                        tags.Add(new NostrEventTag() { TagIdentifier = "q", Data = [rootEvent.Id, targetRelayHint, rootEvent.PublicKey] });
                    }
                    else
                    {
                        string? rootId = null;
                        if (rootEvent.Tags != null)
                        {
                            foreach (var tag in rootEvent.Tags)
                            {
                                if (tag.TagIdentifier == "e" && tag.Data != null && tag.Data.Count > 2 && tag.Data[2] == "root")
                                {
                                    rootId = tag.Data[0];
                                    break;
                                }
                            }

                            if (rootId == null)
                            {
                                foreach (var tag in rootEvent.Tags)
                                {
                                    if (tag.TagIdentifier == "e" && tag.Data != null && tag.Data.Count > 0 && !string.IsNullOrEmpty(tag.Data[0]))
                                    {
                                        rootId = tag.Data[0];
                                        break;
                                    }
                                }
                            }
                        }

                        if (rootId != null)
                        {
                            var rootRelayHint = GetBestRelayHint(rootId);
                            if (string.IsNullOrEmpty(rootRelayHint)) rootRelayHint = targetRelayHint;
                            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootId, rootRelayHint, "root"] });
                            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootEvent.Id, targetRelayHint, "reply"] });
                        }
                        else
                        {
                            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootEvent.Id, targetRelayHint, "root"] });
                        }

                        tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [rootEvent.PublicKey] });
                    }
                }
            }

            if (!string.IsNullOrEmpty(extraMentionHex) &&
                !tags.Any(t => t.TagIdentifier == "p" && t.Data != null && t.Data.Count > 0 && t.Data[0] == extraMentionHex))
            {
                tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [extraMentionHex] });
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
                Kind = eventKind,
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
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            int eventKind = _mode switch
            {
                BotMode.Channel => 42,
                BotMode.BitChat => 20000,
                _ => 1
            };

            if (_mode == BotMode.Channel)
            {
                var channelHex = _channelId.ConvertEventIdToHex();
                if (!string.IsNullOrEmpty(channelHex))
                {
                    var channelRelayHint = GetBestRelayHint(channelHex);
                    tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [channelHex, channelRelayHint, "root"] });
                }
            }
            else if (_mode == BotMode.BitChat)
            {
                var botName = string.IsNullOrWhiteSpace(_botName) ? "まとめbot" : _botName;
                var geohash = string.IsNullOrWhiteSpace(_geohash) ? "xn" : _geohash;
                tags.Add(new NostrEventTag() { TagIdentifier = "n", Data = [botName] });
                tags.Add(new NostrEventTag() { TagIdentifier = "g", Data = [geohash] });
            }

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
                Kind = eventKind,
                Content = (_addNostrNpub1 ? "nostr:" + _director + " " : "") + content.Replace("\r\n", "\n"),
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
            var targetRelayHint = GetBestRelayHint(e);
            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [e, targetRelayHint] });
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
            var targetRelayHint = GetBestRelayHint(e);
            tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [e, targetRelayHint] });
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
            var oldMode = _mode;
            var oldChannelId = _channelId;

            _formSetting.checkBoxTopMost.Checked = TopMost;
            Opacity = _tempOpacity;
            _formSetting.trackBarOpacity.Value = (int)(Opacity * 100);
            _formSetting.checkBoxMinimizeToTray.Checked = _minimizeToTray;
            _formSetting.checkBoxAddClient.Checked = _addClient;

            _formSetting.comboBoxMode.SelectedIndex = (int)_mode;
            _formSetting.textBoxChannelId.Text = _channelId;
            _formSetting.textBoxGeohash.Text = _geohash;
            _formSetting.textBoxBotName.Text = _botName;

            _formSetting.textBoxDirector.Text = _director;
            _formSetting.checkBoxShowOnlyFollowees.Checked = _showOnlyFollowees;
            _formSetting.checkBoxUsePetname.Checked = _usePetname;
            _formSetting.checkBoxSummarizeEveryHour.Checked = _summarizeEveryHour;
            _formSetting.numericUpDownSummarizeMinutes.Value = _summarizeMinutes;
            _formSetting.checkBoxMentionMode.Checked = _mentionMode;
            _formSetting.checkBoxAddNostrNpub1.Checked = _addNostrNpub1;
            _formSetting.checkBoxSummarizeByEventCount .Checked = _summarizeByEventCount;
            _formSetting.numericUpDownEventThreshold.Value = _eventThreshold;
            _formSetting.textBoxForceCommands.Text = string.Join("\r\n", _forceCommands);
            _formSetting.textBoxCallCommands.Text = string.Join("\r\n", _callCommands);
            _formSetting.checkBoxOpenMode.Checked = _openMode;
            _formSetting.checkBoxReactToZaps.Checked = _reactToZaps;
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

            var newMode = (BotMode)_formSetting.comboBoxMode.SelectedIndex;
            var newChannelId = _formSetting.textBoxChannelId.Text;
            bool modeChanged = (oldMode != newMode) || (newMode == BotMode.Channel && oldChannelId != newChannelId);

            _mode = newMode;
            _channelId = newChannelId;
            _geohash = _formSetting.textBoxGeohash.Text;
            _botName = _formSetting.textBoxBotName.Text;

            if (modeChanged)
            {
                dataGridViewNotes.Rows.Clear();
                _displayedEventIds.Clear();
                LastCreatedAt = DateTimeOffset.MinValue;
                LatestCreatedAt = DateTimeOffset.MinValue;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            _director = _formSetting.textBoxDirector.Text;
            _showOnlyFollowees = _formSetting.checkBoxShowOnlyFollowees.Checked;
            _usePetname = _formSetting.checkBoxUsePetname.Checked;
            _summarizeEveryHour = _formSetting.checkBoxSummarizeEveryHour.Checked;
            _summarizeMinutes = (int)_formSetting.numericUpDownSummarizeMinutes.Value;
            _mentionMode = _formSetting.checkBoxMentionMode.Checked;
            _addNostrNpub1 = _formSetting.checkBoxAddNostrNpub1.Checked;
            _summarizeByEventCount = _formSetting.checkBoxSummarizeByEventCount.Checked;
            _eventThreshold = (int)_formSetting.numericUpDownEventThreshold.Value;
            _forceCommands = [.. _formSetting.textBoxForceCommands.Text.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)];
            _callCommands = [.. _formSetting.textBoxCallCommands.Text.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)];
            _openMode = _formSetting.checkBoxOpenMode.Checked;
            _reactToZaps = _formSetting.checkBoxReactToZaps.Checked;
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
                var appTitle = GetAppTitle();
                Text = appTitle;
                notifyIcon.Text = appTitle;

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

                    // タイムラインとZapレシートを再購読
                    await NostrAccess.SubscribeAsync(_mode, _channelId.ConvertEventIdToHex(), _npubHex);

                    // フォロイーを購読をする
                    await NostrAccess.SubscribeFollowsAsync(_director.ConvertToHex());

                    // ログインユーザー名取得
                    var loginName = GetName(_npubHex);
                    var directorName = GetName(_director.ConvertToHex());
                    if (!string.IsNullOrEmpty(loginName))
                    {
                        Text = $"{appTitle} - @{loginName} to {directorName}";
                        notifyIcon.Text = $"{appTitle} - @{loginName} to {directorName}";
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

            Setting.Mode = _mode;
            Setting.ChannelId = _channelId;
            Setting.BotName = _botName;
            Setting.Geohash = _geohash;
            Setting.Director = _director;
            Setting.ShowOnlyFollowees = _showOnlyFollowees;
            Setting.UsePetname = _usePetname;
            Setting.SummarizeEveryHour = _summarizeEveryHour;
            Setting.SummarizeMinutes = _summarizeMinutes;
            Setting.MentionMode = _mentionMode;
            Setting.AddNostrNpub1 = _addNostrNpub1;
            Setting.SummarizeByEventCount = _summarizeByEventCount;
            Setting.EventThreshold = _eventThreshold;
            Setting.ForceCommands = _forceCommands;
            Setting.CallCommands = _callCommands;
            Setting.OpenMode = _openMode;
            Setting.ReactToZaps = _reactToZaps;
            Setting.CallReplyLimit = _callReplyLimit;
            Setting.AppendUserId = _appendUserId;

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
                //Debug.WriteLine($"名前取得: {user.DisplayName} @{user.Name} 📛{user.PetName}");
            }
            return userName;
        }

        /// <summary>
        /// ユーザー表示名と短縮ID（設定有効時）を取得する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ユーザー名 (ID:xxxxxxxx)</returns>
        private string GetAuthorNameWithId(string publicKeyHex)
        {
            if (string.IsNullOrEmpty(publicKeyHex) || publicKeyHex.Length < 8)
            {
                return GetUserName(publicKeyHex);
            }

            var name = GetUserName(publicKeyHex);
            if (name == "???")
            {
                return publicKeyHex[..8];
            }
            if (_appendUserId)
            {
                return $"{name} (ID:{publicKeyHex[..8]})";
            }
            return name;
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

                // AI設定・チャット履歴の保存
                _formAI?.SaveAISettings();

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

                _formAI.InitializeSession();

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
                    // kindが7（リアクション）や9735（Zapレシート）の時はスキップ
                    if ((int)row.Cells["kind"].Value == 7 || (int)row.Cells["kind"].Value == 9735)
                    {
                        continue;
                    }
                    notes.Append(row.Cells["time"].Value?.ToString() + "\r\n");
                    var authorName = row.Cells["name"].Value?.ToString()?.Substring(2) ?? string.Empty;
                    var pubkey = row.Cells["pubkey"].Value?.ToString();
                    if (_appendUserId && !string.IsNullOrEmpty(pubkey) && pubkey.Length >= 8)
                    {
                        notes.Append($"{authorName} (ID:{pubkey[..8]})\r\n");
                    }
                    else
                    {
                        notes.Append(authorName + "\r\n");
                    }
                    notes.Append(row.Cells["note"].Value?.ToString() + "\r\n");
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
            var nextTrigger = new DateTime(now.Year, now.Month, now.Day, now.Hour, _summarizeMinutes, 0);
            if (now.Minute >= _summarizeMinutes)
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
                await NostrAccess.SubscribeAsync(_mode, _channelId.ConvertEventIdToHex(), _npubHex);

                // ログイン済みの時
                if (!string.IsNullOrEmpty(_npubHex))
                {
                    // フォロイーを購読する
                    await NostrAccess.SubscribeFollowsAsync(_director.ConvertToHex());
                }

                labelRelays.Invoke((MethodInvoker)(() => labelRelays.Text = "Reconnected successfully."));

                // ユーザー情報の保存
                Tools.SaveUsers(Users);

                // 定時まとめメンション
                if (_summarizeEveryHour)
                {
                    await SummarizeAndPostAsync();
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

        // まとめ投稿
        private async Task SummarizeAndPostAsync()
        {
            if (_isSummarizing)
            {
                return;
            }
            _isSummarizing = true;

            try
            {
                if (!_formAI.IsInitialized)
                {
                    LastCreatedAt = DateTimeOffset.MinValue;
                    LatestCreatedAt = DateTimeOffset.MinValue;
                }
                bool success = await _formAI.SummarizeNotesAsync();
                if (!success)
                {
                    return;
                }
                // 1秒待つ
                await Task.Delay(1000);
                string answerText = string.Empty;
                Invoke((MethodInvoker)(() => answerText = _formAI.textBoxAnswer.Text.TrimEnd('\r', '\n')));
                // 空文字列の場合は投稿しない
                if (string.IsNullOrEmpty(answerText))
                {
                    _isSummarizing = false;
                    return;
                }
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
            catch (Exception ex)
            {
                Debug.WriteLine($"定時投稿エラー: {ex.Message}");
            }
            finally
            {
                _isSummarizing = false;
            }
        }
        #endregion

        private string GetBestRelayHint(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return string.Empty;

            List<string>? seenOn = null;
            lock (_eventSeenOn)
            {
                if (_eventSeenOn.TryGetValue(eventId, out var list))
                {
                    seenOn = new List<string>(list);
                }
            }

            if (seenOn == null || seenOn.Count == 0)
            {
                return string.Empty;
            }

            if (NostrAccess.Relays != null)
            {
                foreach (var r in NostrAccess.Relays)
                {
                    var rStr = NormalizeRelayUrl(r.ToString());
                    var found = seenOn.FirstOrDefault(s => NormalizeRelayUrl(s) == rStr);
                    if (found != null) return found;
                }
            }

            return seenOn[0];
        }

        private static string NormalizeRelayUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            return url.Trim().TrimEnd('/');
        }
    }
}
