# kako

Tiny nostr summary bot client for windows.

## Usage
- F5キーで開くGemini画面で「Gemini API Key」を入力してください。
- ESCキーで開く設定画面で「Director's npub」に返信先（あなた）のnpubを入力してください。
- 「Private key」にbotのNostr秘密鍵（nsec1...）を入力してください。
- 「Reply commands」にbotが反応するコマンドを入力してください。
- 「Reply commands」に設定したコマンドを投稿すると、botが前回実施以降のタイムラインのまとめを投稿します。

## Tips
- botのnsecはnostterをシークレットウィンドウで開くと手軽に作成できます。
- 「Reply commands」は改行で複数のコマンドを設定できます。
- F5キーで開くGemini画面でbotに投稿させずにまとめのテストやbotとのチャットを行うことができます。
- reset と投稿すると次回まとめ投稿前にModelとInitial promptを再読み込みします。（Initializedチェックボックスを外します）