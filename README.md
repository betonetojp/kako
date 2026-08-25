# kako

Tiny nostr summary bot client for windows.

## 概要 (Overview)
`kako` は Nostr のタイムラインやチャンネルの投稿を定期的に収集し、Google Gemini AI を用いて要約や会話応答を行う Windows 向け Bot クライアントです。

1つのアプリで以下の3つの動作モードに対応しています：
- **Note (Kind 1)**: 通常のタイムラインの要約・返信
- **Channel (Kind 42)**: NIP-28 パブリックチャットチャンネル内の要約・返信
- **BitChat (Kind 20000)**: BitChat チャンネル内の要約・返信

---

## ショートカットキー & マウス操作 (Shortcuts & Mouse)

| 操作 | 動作 |
| :--- | :--- |
| `ESC` | 基本設定画面（FormSetting）を開く |
| `F5` | Gemini AI 設定・対話画面（FormAI）を開く / 閉じる |
| `F2` | タイムラインの「時間（time）」列の表示 / 非表示切替 |
| `F3` | タイムラインの「アバター（avatar）」列の表示 / 非表示切替 |
| `F4` | タイムラインの「名前（name）」列の表示 / 非表示切替 |
| `F9` / **ダブルクリック** | タイムライン本文の折り返し（Wrap）表示の ON / OFF 切替 |
| `F10` / **右クリック** | マニアクス画面（FormManiacs）を開く |
| `Ctrl + Shift + A` | **グローバルホットキー**: アプリ全体の表示 / 非表示トグル |

---

## 初期設定と使い方 (Usage)

### 1. Gemini 設定 (F5キー)
F5キーを押して AI 設定画面を開きます。
- **Gemini API Key**: [Google AI Studio](https://aistudio.google.com/apikey) で取得した API キーを入力してください。
- **Model**: 使用する Gemini モデル名を入力してください（例: `gemini-2.5-flash`, `gemini-3.1-flash-lite` など）。
  - ※ **未設定（空欄）の場合は AI 機能が動作しません。**
- **Initial prompt**: AI の初期化時に送信されるプロンプトです（ペルソナや基本ルールを設定）。
- **Prompt for every message**: 定期要約時にタイムライン内容の先頭に付加される指示文面です。
- **Prompt for reply**: リプライ応答時に付加される指示文面です。
- **Turns**: 保持する会話履歴のターン数（往復数）です。
- **Number of posts to load**: 要約作成時に読み込む過去投稿の最大件数です。
- **Initialized**: AI セッションの初期化状態を示します。
  - **会話履歴のリセット方法**: チェックを手動で外して「Summarize」ボタンを押すと、過去の会話履歴（`chatSession.json`）をリセットして初期プロンプトからやり直すことができます。

### 2. 基本設定 (ESCキー)
ESCキーを押して設定画面を開きます。
- **Always on top**: ウィンドウを常に最前面に表示します。
- **Opacity**: ウィンドウの不透明度を調整します。
- **Minimize to system tray**: ウィンドウの閉じる（×）ボタンを押した際、終了せずにタスクトレイに格納します。
- **Add client tag**: 投稿に `client: ["kako"]` タグを付加します。
- **Mode**: 動作モードを選択します（`Note (Kind 1)` / `Channel (Kind 42)` / `BitChat (Kind 20000)`）。
  - ※モードを切り替えて設定を閉じると、取得済み投稿一覧がクリアされ、新モードで再購読を開始します。
- **Channel ID**: Channel モード選択時に対象チャンネルのイベントIDを入力します（`note1...`, `nevent1...`, または 64桁 hex）。
- **Geohash**: BitChat モード選択時に対象地域の Geohash コードを入力します（`g` タグ、初期値: `xn`）。
- **Bot name**: BitChat モード選択時に Bot の表示名/ニックネームを入力します（`n` タグ、初期値: `まとめbot`）。
- **Director's npub**: 返信先（マスター/管理者）の npub を入力します。
- **Private key**: Bot の Nostr 秘密鍵（`nsec1...`）を入力します。
- **Show only followees**: 有効にすると、返信先（Director）がフォローしているユーザーのみのイベントを取得します。
- **Use petname**: 返信先（Director）が設定している petname を表示名として使用します。
- **Summarize every hour at xx minutes**: 毎時 xx 分に前回収集以降のイベントを要約して投稿します。
- **Summarize every xxx events**: 指定したイベント件数を受信するごとに要約して投稿します。
- **Mention**: 要約投稿時に Director へのメンション（`p` タグ）を付加します。
- **Add nostr:npub1...**: 要約投稿の本文先頭に `nostr:<Director's npub>` を追加します。
- **Force commands**: Director 専用の強制要約コマンドを設定します（改行で複数指定可）。
- **Call commands**: Bot を呼び出して会話応答させるコマンドを設定します（改行で複数指定可、前方一致で引数付き呼び出しに対応）。
- **Open mode**: 有効にすると、Director 以外のユーザーからのリプライ・呼び出しにも応答します。
- **Stamina**: 応答可能な最大回数を設定します。要約投稿時や設定画面開閉時にリセットされます。

### 3. リレー設定
- 画面右下の **リストアイコンボタン（📄）** をクリックすると、リレー設定画面が開きます。
- 接続したいリレーの有効 / 無効の切り替えや追加・削除が可能です（設定は `relays.json` に保存されます）。

---

## コマンド & Tips
- **Bot 用 nsec**: [nostter](https://nostter.app) をシークレットウィンドウで開くと手軽に新規作成できます。
- **設定画面のコマンド指定**: 「Force commands」「Call commands」は改行で複数のキーワードを設定できます。
- **F5 画面でのテスト**: Gemini 画面上で「Summarize」ボタンを押すと、Nostr へ投稿せずに要約結果をテスト確認できます。「Chat」ボタンで直接対話も可能です。
- **Director 専用リモートコマンド** (Bot へのリプライで実行可能):
  - `reset`: AI の会話セッションをリセットし、次回要約時に Model と Initial prompt を再読み込みします。
  - `start`: 「Summarize every hour at xx minutes」を有効化します。
  - `start2`: 「Summarize every xxx events」を有効化します。
  - `stop`: すべての定期要約を無効化します。
  - `open`: 「Open mode」を有効化します。
  - `close`: 「Open mode」を無効化します。
  - `clear`: 受信中のイベント一覧（画面表示）をクリアします。

---

## `AI.json`（UI からは編集できない詳細設定）
一部の詳細設定はアプリの実行ディレクトリにある `AI.json` に保存されます。

- `SleepStartHour` / `SleepEndHour`:
  - 指定した時間帯は要約を実行しない（スリープ）時間帯です（例: `SleepStartHour: 23`, `SleepEndHour: 6` で 23時〜翌6時はスリープ）。
- `UseGoogleSearch`:
  - 生成モデルが Google 検索グラウンディングを利用するかどうかのフラグ（`true` / `false`）。
- `CommunicationErrorMessage`:
  - AI との通信に失敗した際に表示・投稿されるエラーメッセージ。

これらを変更する場合は、アプリを終了した状態で `AI.json` を編集して保存し、アプリを再起動してください。

---

## 利用ライブラリ / NuGet パッケージ
- [CredentialManagement](https://www.nuget.org/packages/CredentialManagement)
- [Google_GenerativeAI](https://www.nuget.org/packages/Google_GenerativeAI)
- [NNostr](https://github.com/Kukks/NNostr) (一部カスタマイズして同梱)
