# 走れダチョウ

サイキッカーの主人公が、念でダチョウに命令しながら追っ手から逃げるランゲーム。
ダチョウは命令を2秒で忘れる。命令しすぎると主人公の頭がダチョウになる。

---

## Unityでの起動方法

### 必要なもの
- Unity Hub
- Unity 2022.3 LTS 以降（Unity 6 推奨）
- Git

### 最初の1回だけ：Unityプロジェクトとして開けるようにする
このリポジトリには `Assets/` しか入っていないので、最初に一度だけUnityの設定ファイルを作ります。

1. リポジトリをクローンする
   ```
   git clone https://github.com/kuramoto1016/datyou.git
   ```
2. Unity Hub で「新しいプロジェクト」→ テンプレート「3D」または「Universal 3D」を選ぶ
   - 保存場所は **どこでもいい**（例：`Desktop/datyou-tmp`）
3. 作ったプロジェクトをいったん閉じて、そのフォルダの中の `Packages/` と `ProjectSettings/` を、クローンした `datyou/` フォルダにコピーする
4. Unity Hub で「追加」→「ディスクから加える」→ クローンした `datyou/` フォルダを選んで開く
5. （不要になりました）Active Input Handling は既定のままでOK。新Input System／旧Input Manager／Both のどれでも動きます
6. `Packages/` と `ProjectSettings/` をコミットしてpushする
   ```
   git add Packages ProjectSettings
   git commit -m "Unityプロジェクト設定を追加"
   git push
   ```
   これで次回からは手順4だけで開けます。

### 遊ぶ
1. Project ウィンドウで右クリック → Create → Scene で新しいシーンを作って開く
2. Hierarchy で右クリック →「Create Empty」で空の GameObject を作る
3. Inspector の「Add Component」で `GameBootstrap` を付ける
4. ▶ で再生（ダチョウ・コース・カメラ・HUDは全部自動で作られます）
5. Game ビューを一度クリックしてから操作する

| 入力 | 命令 |
|---|---|
| ← / A / 左スワイプ | 左へ |
| → / D / 右スワイプ | 右へ |
| ↑ / W / Space / 上スワイプ | ジャンプ |
| ↓ / S / タップ | 「そのまま！」（本能を2秒黙らせる） |
| R / クリック（ゲームオーバー後） | リトライ |

スワイプはエディタ上ではマウスのドラッグで試せます。

### よくあるトラブル
- **再生すると `InvalidOperationException` が出て操作できない** → 古いブランチのコードかもしれない。`main` を最新にする。それでも出る場合はエラー全文をPRかIssueに貼ってください
- **`FindObjectOfType` が古いという黄色い警告** → 動作には影響なし
- **オブジェクトがピンク色** → シェーダーが見つかっていない。Issue かPRコメントでUnityのバージョンを教えてください

---

## 開発の進め方
Claude が定期的に `TODO.md` の続きを実装し、コードレビューをして問題がなければPRを出します。ルールは `CLAUDE.md` を参照。

PRの確認方法：
1. PRのブランチをチェックアウトする（`git fetch && git checkout <ブランチ名>`）
2. Unityで開いて、PR本文の「確認手順」どおりに動かす
3. 問題なければマージ。問題があればPRにコメント（次の実行で Claude が直します）
