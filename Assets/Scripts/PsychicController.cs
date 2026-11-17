using UnityEngine;

/// <summary>
/// 主人公（サイキッカー）。入力を受けてダチョウに念で命令する。
/// 命令するたびに頭がダチョウ化（ハゲ度が上がる）。100%でバカになってゲームオーバー。
/// </summary>
public class PsychicController : MonoBehaviour
{
    public OstrichController ostrich;

    [Header("ハゲ度")]
    public float maxBaldness = 100f;
    public float costPerCommand = 8f;
    public float costJump = 10f;              // 強い命令ほど禿げる
    public float recoverPerSec = 1.5f;        // 命令を我慢すると少し戻る（0で戻らない）
    public float recoverDelay = 2f;           // 最後の命令から何秒後に戻り始めるか

    [Header("スワイプ")]
    public float swipeMinPixels = 60f;

    public float Baldness { get; private set; }
    public float Baldness01 => Baldness / maxBaldness;
    public bool IsOstrichHead { get; private set; }

    float sinceLastCommand;
    Vector2 touchStart;
    bool touching;

    void Update()
    {
        if (IsOstrichHead || ostrich == null || !ostrich.Alive) return;

        sinceLastCommand += Time.deltaTime;
        if (sinceLastCommand > recoverDelay)
            Baldness = Mathf.Max(0f, Baldness - recoverPerSec * Time.deltaTime);

        if (TryReadCommand(out OstrichCommand cmd))
            Command(cmd);
    }

    void Command(OstrichCommand cmd)
    {
        ostrich.ReceiveCommand(cmd);
        Baldness += cmd == OstrichCommand.Jump ? costJump : costPerCommand;
        sinceLastCommand = 0f;

        if (Baldness >= maxBaldness)
        {
            Baldness = maxBaldness;
            IsOstrichHead = true;
            ostrich.Kill(); // 主人公がバカになったので逃走終了
        }
    }

    // ---------- 入力（キーボード＋スワイプ。実際の読み取りは GameInput） ----------

    bool TryReadCommand(out OstrichCommand cmd)
    {
        cmd = OstrichCommand.Stay;

        if (GameInput.LeftPressed) { cmd = OstrichCommand.Left; return true; }
        if (GameInput.RightPressed) { cmd = OstrichCommand.Right; return true; }
        if (GameInput.JumpPressed) { cmd = OstrichCommand.Jump; return true; }
        if (GameInput.StayPressed) { cmd = OstrichCommand.Stay; return true; }

        // スマホ：左右スワイプ＝レーン移動、上スワイプ＝ジャンプ、タップ＝「そのまま！」
        // （マウスでもエディタ上で試せる）
        if (GameInput.PointerDown) { touchStart = GameInput.PointerPosition; touching = true; }
        if (touching && GameInput.PointerUp)
        {
            touching = false;
            Vector2 delta = GameInput.PointerPosition - touchStart;
            if (delta.magnitude < swipeMinPixels) { cmd = OstrichCommand.Stay; return true; }
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                cmd = delta.x < 0 ? OstrichCommand.Left : OstrichCommand.Right;
            else if (delta.y > 0)
                cmd = OstrichCommand.Jump;
            else
                cmd = OstrichCommand.Stay;
            return true;
        }
        return false;
    }
}
