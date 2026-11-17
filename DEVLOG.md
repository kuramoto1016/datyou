# 開発ログ

## 2026-11-17
- リポジトリを初期化。プロトタイプのスクリプト5つ、開発ルール（CLAUDE.md）、TODO.md を追加
- Kuraに確認してほしいこと：README の手順でUnityで開けるか
- `FindObjectOfType<Light>()` を `FindAnyObjectByType<Light>()` に置き換えた（ブランチ `claude/2026-11-17-find-object`）。リポジトリ内の使用箇所はこの1か所だけ
  - TODOには `FindFirstObjectByType` と書いていたが、「ライトが1つでもあるか」を見るだけなので並べ替えをしない速い方を選んだ
  - 静的チェック：mcsで全スクリプトの構文チェック、スタブで該当呼び出しのコンパイルを確認
  - PR #1 と TODO.md / DEVLOG.md が近い行を触るので、後からマージする方でコンフリクトが出るかもしれない
- Kuraに確認してほしいこと：Unity 6で再生してコンソールに `FindObjectOfType is obsolete` の警告が出ないこと、空のシーンでもライトが付くこと
