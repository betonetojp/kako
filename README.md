# kako

Tiny nostr summary bot client for windows.

## Usage
- F5キーで開くGemini画面で「Gemini API Key」を入力してください。
- ESCキーで開く設定画面で「Director's npub」に返信先（あなた）のnpubを入力してください。
- 「Private key」にbotのNostr秘密鍵（nsec1...）を入力してください。
- 「Show only followees」を有効にすると、返信先（あなた）がフォローしているユーザーのみのタイムラインを取得します。
- 「Use petname」を有効にすると、返信先（あなた）が設定しているpetnameを使用します。
- 「Summarize every hour at xx minutes」を有効にすると、毎時xx分にbotが前回実施以降のタイムラインのまとめを投稿します。
- 「Summarize every xxx events」を有効にすると、イベントをxxx行読み込むごとにbotが前回実施以降のタイムラインのまとめを投稿します。
- 「Mention」を有効にすると、まとめの投稿がメンションになります。
- 「Add nostr:npub1...」を有効にすると、まとめ投稿の先頭に「nostr:<Director's npub>」を追加します。
- 「Force commands」にbotが反応するコマンドを入力してください。
- 「Force commands」に設定したコマンドを投稿すると、botが前回実施以降のタイムラインのまとめを投稿します。(Director専用)
- 「Call commands」にbotを呼び出すコマンドを入力してください。
- 「Open mode」を有効にすると、あなた以外の返信にも応答します。
- 「Stamina」で返信に応答する上限回数を設定できます。
- 返信するとGeminiに「Chat」を送信し、結果を返信投稿します。

## Tips
- botのnsecは[nostter](https://nostter.app)をシークレットウィンドウで開くと手軽に作成できます。
- 「Force commands」「Call commands」は改行で複数のコマンドを設定できます。
- F5キーで開くGemini画面でbotに投稿させずにまとめのテストやbotとのチャットを行うことができます。
- reset と返信すると「Initialized」を外し、次回まとめ投稿前に「Model」と「Initial prompt」を再読み込みします。(Director専用)
- start と返信すると「Summarize every hour at xx minutes」を有効にします。(Director専用)
- stop と返信すると「Summarize every hour at xx minutes」を無効にします。(Director専用)
- open と返信すると「Open mode」を有効にします。(Director専用)
- close と返信すると「Open mode」を無効にします。(Director専用)
- 「Stamina」は毎時xx分にまとめを投稿した際と設定画面を開閉した時にリセットされます。
- 「Director's npub」が設定されていないとDirector専用コマンドに反応しません。

## 利用NuGetパッケージ
- [CredentialManagement](https://www.nuget.org/packages/CredentialManagement)
- [Google_GenerativeAI](https://www.nuget.org/packages/Google_GenerativeAI)

## Nostrクライアントライブラリ
- [NNostr](https://github.com/Kukks/NNostr) 内のNNostr.Client Ver0.0.49を一部変更して利用しています。
