# kako

Tiny nostr summary bot client for windows.

## Usage
- F5キーで開くGemini画面で「Gemini API Key」を入力してください。
- ESCキーで開く設定画面で「Director's npub」に返信先（あなた）のnpubを入力してください。
- 「Private key」にbotのNostr秘密鍵（nsec1...）を入力してください。
- 「Show only followees」を有効にすると、返信先（あなた）がフォローしているユーザーのみのタイムラインを取得します。
- 「Use petname」を有効にすると、返信先（あなた）が設定しているpetnameを使用します。
- 「Summarize every hour at xx minutes」を有効にすると、毎時xx分にbotが前回実施以降のタイムラインのまとめを投稿します。
- 「Mention」を有効にすると、まとめの投稿がメンションになります。
- 「Force commands」にbotが反応するコマンドを入力してください。
- 「Force commands」に設定したコマンドを投稿すると、botが前回実施以降のタイムラインのまとめを投稿します。(Director専用)
- 「Call commands」にbotを呼び出すコマンドを入力してください。
- 「Open mode」を有効にすると、誰の返信にも反応します。
- 「Stamina」で返信に反応する上限回数を設定できます。
- 返信するとGeminiに「Chat」を送信し、結果を返信投稿します。
- 

## Tips
- botのnsecはnostterをシークレットウィンドウで開くと手軽に作成できます。
- 「Force commands」「Call commands」は改行で複数のコマンドを設定できます。
- F5キーで開くGemini画面でbotに投稿させずにまとめのテストやbotとのチャットを行うことができます。
- reset と返信すると「Initialized」を外し、次回まとめ投稿前に「Model」と「Initial prompt」を再読み込みします。(Director専用)
- start と返信すると「Summarize every hour at xx minutes」を有効にします。(Director専用)
- stop と返信すると「Summarize every hour at xx minutes」を無効にします。(Director専用)
- 「Stamina」は毎時xx分にまとめを投稿した際と設定画面を開閉した時にリセットされます。
- 「Director's npub」が設定されていないとDirector専用コマンドに反応しません。