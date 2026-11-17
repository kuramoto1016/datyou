using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 入力の窓口。新Input System（ENABLE_INPUT_SYSTEM）と旧Input Manager（ENABLE_LEGACY_INPUT_MANAGER）の
/// どちらが有効でも動くようにする。Active Input Handling の設定を変えなくてよい。
/// - 新しい方が使えるなら新しい方を使う（Unity 6 の新規プロジェクトの既定）
/// - そうでなければ旧Input Manager
/// ゲーム側はここだけを呼ぶ（UnityEngine.Input を直接触らない）。
/// </summary>
public static class GameInput
{
    public static bool LeftPressed =>
        AnyKeyDown(Keys.Left);

    public static bool RightPressed =>
        AnyKeyDown(Keys.Right);

    public static bool JumpPressed =>
        AnyKeyDown(Keys.Jump);

    public static bool StayPressed =>
        AnyKeyDown(Keys.Stay);

    public static bool RetryKeyPressed =>
        AnyKeyDown(Keys.Retry);

    // ---------- ポインター（マウス／タッチ。スワイプ判定に使う） ----------

#if ENABLE_INPUT_SYSTEM
    /// <summary>このフレームで押された（マウス左ボタン or 1本目の指）</summary>
    public static bool PointerDown => Pointer.current != null && Pointer.current.press.wasPressedThisFrame;

    /// <summary>このフレームで離された</summary>
    public static bool PointerUp => Pointer.current != null && Pointer.current.press.wasReleasedThisFrame;

    /// <summary>スクリーン座標（左下が原点。旧Input.mousePositionと同じ向き）</summary>
    public static Vector2 PointerPosition => Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;

    static bool AnyKeyDown(Key[] keys)
    {
        var kb = Keyboard.current;
        if (kb == null) return false; // スマホなどキーボードが無い環境
        for (int i = 0; i < keys.Length; i++)
            if (kb[keys[i]].wasPressedThisFrame) return true;
        return false;
    }

    static class Keys
    {
        public static readonly Key[] Left = { Key.LeftArrow, Key.A };
        public static readonly Key[] Right = { Key.RightArrow, Key.D };
        public static readonly Key[] Jump = { Key.UpArrow, Key.W, Key.Space };
        public static readonly Key[] Stay = { Key.DownArrow, Key.S };
        public static readonly Key[] Retry = { Key.R };
    }
#elif ENABLE_LEGACY_INPUT_MANAGER
    // 旧Input Managerはタッチをマウスとしても報告する（Input.simulateMouseWithTouches の既定値 true）
    public static bool PointerDown => Input.GetMouseButtonDown(0);
    public static bool PointerUp => Input.GetMouseButtonUp(0);
    public static Vector2 PointerPosition => Input.mousePosition;

    static bool AnyKeyDown(KeyCode[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
            if (Input.GetKeyDown(keys[i])) return true;
        return false;
    }

    static class Keys
    {
        public static readonly KeyCode[] Left = { KeyCode.LeftArrow, KeyCode.A };
        public static readonly KeyCode[] Right = { KeyCode.RightArrow, KeyCode.D };
        public static readonly KeyCode[] Jump = { KeyCode.UpArrow, KeyCode.W, KeyCode.Space };
        public static readonly KeyCode[] Stay = { KeyCode.DownArrow, KeyCode.S };
        public static readonly KeyCode[] Retry = { KeyCode.R };
    }
#else
    // どちらの入力も無効な設定（通常ありえない）。コンパイルだけは通す
    public static bool PointerDown => false;
    public static bool PointerUp => false;
    public static Vector2 PointerPosition => Vector2.zero;

    static bool AnyKeyDown(int[] keys) => false;

    static class Keys
    {
        public static readonly int[] Left = { }, Right = { }, Jump = { }, Stay = { }, Retry = { };
    }
#endif
}
