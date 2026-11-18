using UnityEngine;

/// <summary>
/// 後ろから迫る追っ手のドローン。
/// - ダチョウとの「距離（Gap）」を持つ。障害物に当たってダチョウがよろけるたびに縮む
/// - 何事もなく走っていれば少しずつ引き離せる（ただし最大距離まで）
/// - 追いつかれる（Gap が 0）と捕獲ビームで大ダメージ → 少し距離が開く
///   ※ゲームオーバー条件そのもの（体力0／ハゲ100%）は変えていない。即ゲームオーバーにするかは要相談
/// - ダチョウが倒れたら真上に来てビームを当てる（ゲームオーバー演出）
/// </summary>
public class PursuerDrone : MonoBehaviour
{
    public OstrichController ostrich;

    [Header("距離")]
    public float maxGap = 20f;
    public float startGap = 14f;
    public float gapLossPerHit = 6f;          // 障害物に当たるとこれだけ詰められる
    public float gapRecoverPerSec = 0.4f;     // 順調に走っていると引き離せる
    public float dangerGap = 6f;              // HUDで警告を出す距離

    [Header("捕まったとき")]
    public float catchDamage = 40f;
    public float gapAfterCatch = 8f;
    public float beamShowTime = 0.6f;

    [Header("見た目の位置（カメラに映る範囲に縮めて表示する）")]
    // 画面の下端（ダチョウの足元より下）に映る位置。距離が縮むほど画面の上＝ダチョウの足元へ迫ってくる。
    // カメラ（後ろ8m・高さ5m・約16°見下ろし・縦FOV60°）から見て、画面中心より約18°下（近）〜約26°下（遠）。
    // ダチョウ（約5〜16°下）や前方のレーンには重ならない
    public float visualBehindNear = 2.5f;     // Gap=0 のときダチョウの何m後ろに見せるか
    public float visualBehindFar = 4f;        // Gap=max のとき
    public float visualHeightNear = 1.3f;
    public float visualHeightFar = 1.4f;
    public float visualScale = 0.6f;
    public float overheadHeight = 3f;         // 捕まえるとき／ゲームオーバー時に真上に来る高さ

    public float Gap { get; private set; }
    public float Gap01 => maxGap > 0f ? Gap / maxGap : 0f;
    public bool InDanger => Gap < dangerGap;
    /// とどめを刺したのがドローンのビームか（ゲームオーバー画面の文言用）
    public bool KilledOstrich { get; private set; }

    Transform[] rotors;
    GameObject beam;
    float beamTimer;
    float bobTime;
    float shownGap01;     // 見た目用（急にワープしないようにゆっくり追従）
    float overhead;       // 0=後ろを追跡 1=ダチョウの真上
    float smoothX;

    void Awake()
    {
        // OnGUI が Start より先に呼ばれても距離0（＝接近警告）にならないように Awake で初期化
        Gap = startGap;
        shownGap01 = Gap01;
    }

    void Start()
    {
        var model = Visuals.CreateDrone(out rotors, out beam);
        model.transform.SetParent(transform, false);
        model.transform.localScale = Vector3.one * visualScale;

        if (ostrich != null)
        {
            ostrich.OnHit += HandleOstrichHit;
            smoothX = ostrich.transform.position.x * 0.5f;
            transform.position = TargetPosition();
        }
    }

    void OnDestroy()
    {
        if (ostrich != null) ostrich.OnHit -= HandleOstrichHit;
    }

    void Update()
    {
        if (ostrich == null) return;
        float dt = Time.deltaTime;

        if (ostrich.Alive)
        {
            Gap = Mathf.Min(maxGap, Gap + gapRecoverPerSec * dt);
            if (beamTimer > 0f) beamTimer -= dt;
        }
        else
        {
            // ゲームオーバー演出：距離0まで詰める
            Gap = Mathf.MoveTowards(Gap, 0f, maxGap * dt);
        }
    }

    // 位置はダチョウが動いた後（LateUpdate）に決める。Update の実行順に左右されず、z もぴったり付いてくる
    void LateUpdate()
    {
        if (ostrich == null) return;
        float dt = Time.deltaTime;
        bobTime += dt;

        foreach (var r in rotors) r.Rotate(0f, 1500f * dt, 0f, Space.Self);

        bool overheadWanted = !ostrich.Alive || beamTimer > 0f;
        overhead = Mathf.MoveTowards(overhead, overheadWanted ? 1f : 0f, 6f * dt);
        shownGap01 = Mathf.MoveTowards(shownGap01, Gap01, 1.5f * dt);
        beam.SetActive(overhead > 0.8f);

        // 横はダチョウを半分だけ、少し遅れて追う（ふらふら追跡している感じ）
        float chaseX = ostrich.transform.position.x * 0.5f + Mathf.Sin(bobTime * 0.9f) * 0.3f;
        smoothX = Mathf.Lerp(smoothX, chaseX, 1f - Mathf.Exp(-4f * dt));

        transform.position = TargetPosition();
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(bobTime * 2.3f) * 6f);
    }

    Vector3 TargetPosition()
    {
        Vector3 p = ostrich.transform.position;
        float behind = Mathf.Lerp(visualBehindNear, visualBehindFar, shownGap01);
        float height = Mathf.Lerp(visualHeightNear, visualHeightFar, shownGap01);
        behind = Mathf.Lerp(behind, 0f, overhead);
        height = Mathf.Lerp(height, overheadHeight, overhead);
        float bob = Mathf.Sin(bobTime * 3f) * 0.1f;

        // 横は smoothX（LateUpdate で追従）。真上に来るときはダチョウに合わせる
        float x = Mathf.Lerp(smoothX, p.x, overhead);
        return new Vector3(x, height + bob, p.z - behind);
    }

    void HandleOstrichHit()
    {
        if (ostrich == null || !ostrich.Alive) return;
        Gap -= gapLossPerHit;
        if (Gap > 0f) return;

        // 追いつかれた：捕獲ビーム
        beamTimer = beamShowTime;
        ostrich.ApplyDamage(catchDamage);
        if (!ostrich.Alive) KilledOstrich = true;
        Gap = gapAfterCatch;
    }
}
