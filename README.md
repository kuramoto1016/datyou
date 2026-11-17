# 走れダチョウ

サイキッカーの主人公が、念でダチョウに命令しながら追っ手から逃げるランゲーム。
ダチョウは命令を2秒で忘れる。命令しすぎると主人公の頭がダチョウになる。

## 最初の1回だけ（Unityプロジェクトにする）
1. このリポジトリをクローンする
2. Unity Hub で「新しいプロジェクト」→「3D」または「Universal 3D」を選び、**このクローンしたフォルダと同じ場所** に作る
   （または、別の場所に作った新規プロジェクトの `Packages/` と `ProjectSettings/` をこのフォルダにコピーする）
3. Edit → Project Settings → Player → Other Settings →「Active Input Handling」を「Both」にする
4. `Packages/` と `ProjectSettings/` をコミットしてpushする（以後は不要）

## 遊び方
1. 新しいシーンを開き、空の GameObject に `GameBootstrap` を付けて再生する
2. ←→ / スワイプ：左右　↑ / Space / 上スワイプ：ジャンプ　↓ / タップ：「そのまま！」
3. ゲームオーバー後は R / クリックでリトライ

## 開発の進め方
Claude が定期的に `TODO.md` の続きを実装してPRを出します。ルールは `CLAUDE.md` を参照。
PRをUnityで確認して、問題なければマージ、問題があればPRにコメントしてください。
