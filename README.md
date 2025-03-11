# kako

Tiny nostr summary bot client for windows.

## Usage
- F5キーで開くGemini画面で「Gemini API Key」を入力してください。
- ESCキーで開く設定画面で「Director's npub」に返信先（あなた）のnpubを入力してください。
- 「Private key」にbotのNostr秘密鍵（nsec1...）を入力してください。
- 「Reply commands」にbotが反応するコマンドを入力してください。
- 「Reply commands」に設定したコマンドを投稿すると、botが前回実施以降のタイムラインのまとめを投稿します。(Director専用)
- 「Call command」にbotを呼び出すコマンドを入力してください。
- 「Open mode」を有効にすると、誰の返信にも反応します。
- 返信するとGeminiに「Chat」を送信し、結果を返信投稿します。
- 「Mention every hour at xx minutes」を有効にすると、毎時xx分にbotが前回実施以降のタイムラインのまとめを投稿します。
- 「Stamina」で返信に反応する上限回数を設定できます。

## Tips
- botのnsecはnostterをシークレットウィンドウで開くと手軽に作成できます。
- 「Reply commands」は改行で複数のコマンドを設定できます。
- F5キーで開くGemini画面でbotに投稿させずにまとめのテストやbotとのチャットを行うことができます。
- reset と投稿すると「Initialized」を外し、次回まとめ投稿前に「Model」と「Initial prompt」を再読み込みします。(Director専用)
- 「Stamina」は毎時xx分にまとめを投稿した際にリセットされます。
- 「Director's npub」が設定されていないと「Reply commands」や reset に反応しません。