# 開発ログ

## 2026-11-17
- リポジトリを初期化。プロトタイプのスクリプト5つ、開発ルール（CLAUDE.md）、TODO.md を追加
- Kuraに確認してほしいこと：README の手順でUnityで開けるか
- 入力を `GameInput` にまとめ、新Input System／旧Input Manager／Both のどれでも動くようにした（ブランチ `claude/2026-11-17-input-both`）
  - `PsychicController` と `GameBootstrap` は `UnityEngine.Input` を直接使わなくなった
  - 静的チェック：スタブを使って4通りの定義（新のみ・旧のみ・両方・なし）で `GameInput.cs` をcscでコンパイルし、エラーなし
  - レビュー（サブエージェント）：ブロッキングな問題なし。新Input Systemで超短いタップを取りこぼす可能性が少しあるのでTODOに追加
- Kuraに確認してほしいこと：Active Input Handling を「Input System Package (New)」のままでキー・ドラッグ・リトライが効くか。READMEの手順5（Bothにする）は不要になった
