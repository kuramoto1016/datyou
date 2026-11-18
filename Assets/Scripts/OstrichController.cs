using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum OstrichCommand { Left, Right, Jump, Stay }

/// <summary>
/// ダチョウ本体。命令は2秒で忘れ、あとは本能で動く。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class OstrichController : MonoBehaviour
{
    [Header("走り")]
    public float baseSpeed = 10f;
    public float speedGainPerMeter = 0.01f;   // 進むほど速くなる
    public float maxSpeed = 25f;
    public float laneWidth = 2f;
    public float laneChangeSpeed = 12f;

    [Header("ジャンプ")]
    public float jumpVelocity = 8f;
    public float gravity = 25f;

    [Header("命令の記憶")]
    public float commandMemory = 2f;          // 2秒で忘れる

    [Header("本能")]
    public float thinkInterval = 0.4f;        // 何秒ごとに考えるか
    public float lookAhead = 12f;             // どこまで前を見るか
    public float panicDistance = 6f;          // これより近い障害物でパニック
    [Range(0, 1)] public float greed = 0.35f;          // エサに釣られる確率
    [Range(0, 1)] public float panicJumpChance = 0.3f; // パニック時にジャンプする確率（壁にも跳ぶ）
    [Range(0, 1)] public float wanderChance = 0.05f;   // なんとなくレーン移動する確率

    [Header("体力")]
    public float maxHp = 100f;
    public float hitDamage = 34f;
    public float regenPerSec = 4f;            // ダチョウはタフなので勝手に治る
    public float invincibleTime = 1f;

    public float Hp { get; private set; }
    public bool Alive { get; private set; } = true;
    public bool IsCommanded => commandTimer > 0f;
    public float CommandTimeLeft => commandTimer;
    public float Distance => transform.position.z - startZ;

    public event Action OnDeath;
    /// 障害物に当たったとき（追っ手が距離を詰めるのに使う）
    public event Action OnHit;

    int lane;                 // -1:左 0:中央 1:右
    float commandTimer;
    float thinkTimer;
    float verticalVel;
    float groundY;
    float startZ;
    float invincibleTimer;
    Renderer[] renderers;

    bool IsGrounded => transform.position.y <= groundY + 0.01f;

    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        renderers = GetComponentsInChildren<Renderer>();
        groundY = transform.position.y;
        startZ = transform.position.z;
        Hp = maxHp;
    }

    void Update()
    {
        if (!Alive) return;
        float dt = Time.deltaTime;

        Hp = Mathf.Min(maxHp, Hp + regenPerSec * dt);
        UpdateInvincible(dt);

        if (commandTimer > 0f)
        {
            commandTimer -= dt;
            if (commandTimer <= 0f)
            {
                // 忘れた瞬間に本能が働きだす
                commandTimer = 0f;
                thinkTimer = 0f;
            }
        }
        else
        {
            thinkTimer -= dt;
            if (thinkTimer <= 0f)
            {
                thinkTimer = thinkInterval;
                Think();
            }
        }

        Move(dt);
    }

    // ---------- 命令 ----------

    public void ReceiveCommand(OstrichCommand cmd)
    {
        if (!Alive) return;
        switch (cmd)
        {
            case OstrichCommand.Left: SetLane(lane - 1); break;
            case OstrichCommand.Right: SetLane(lane + 1); break;
            case OstrichCommand.Jump: Jump(); break;
            case OstrichCommand.Stay: break; // 「そのまま！」本能を2秒黙らせる
        }
        commandTimer = commandMemory;
    }

    // ---------- 本能 ----------

    void Think()
    {
        Scan(lane, out float hazardHere, out float foodHere);

        // 1. 目の前に障害物 → パニック（最善手を選ぶとは限らない）
        if (hazardHere < panicDistance)
        {
            if (Random.value < panicJumpChance) Jump();
            else SetLane(lane + (Random.value < 0.5f ? -1 : 1)); // 端だと曲がれず直進することも
            return;
        }

        // 2. 隣のレーンにエサ → 釣られる（その先に何があっても）
        Scan(lane - 1, out _, out float foodLeft);
        Scan(lane + 1, out _, out float foodRight);
        float nearestFood = Mathf.Min(foodLeft, foodRight);
        if (nearestFood < foodHere && nearestFood < lookAhead && Random.value < greed)
        {
            SetLane(foodLeft <= foodRight ? lane - 1 : lane + 1);
            return;
        }

        // 3. なんとなくフラつく
        if (Random.value < wanderChance)
            SetLane(lane + (Random.value < 0.5f ? -1 : 1));
    }

    /// 指定レーンの前方にある一番近い障害物・エサまでの距離（なければ無限大）
    void Scan(int l, out float hazard, out float food)
    {
        hazard = food = float.PositiveInfinity;
        if (l < -1 || l > 1) return;

        Vector3 center = new Vector3(l * laneWidth, 1f, transform.position.z + lookAhead * 0.5f);
        Vector3 half = new Vector3(laneWidth * 0.4f, 1.5f, lookAhead * 0.5f);
        var hits = Physics.OverlapBox(center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);

        foreach (var h in hits)
        {
            float d = h.transform.position.z - transform.position.z;
            if (d < 0f) continue;
            if (h.TryGetComponent<Hazard>(out _)) hazard = Mathf.Min(hazard, d);
            else if (h.TryGetComponent<Food>(out _)) food = Mathf.Min(food, d);
        }
    }

    // ---------- 移動 ----------

    void SetLane(int l) => lane = Mathf.Clamp(l, -1, 1);

    void Jump()
    {
        if (IsGrounded) verticalVel = jumpVelocity;
    }

    void Move(float dt)
    {
        float speed = Mathf.Min(maxSpeed, baseSpeed + Distance * speedGainPerMeter);
        Vector3 p = transform.position;

        p.z += speed * dt;
        p.x = Mathf.MoveTowards(p.x, lane * laneWidth, laneChangeSpeed * dt);

        verticalVel -= gravity * dt;
        p.y += verticalVel * dt;
        if (p.y <= groundY)
        {
            p.y = groundY;
            verticalVel = 0f;
        }

        transform.position = p;
    }

    // ---------- 当たり判定・体力 ----------

    void OnTriggerEnter(Collider other)
    {
        if (!Alive) return;

        if (other.TryGetComponent(out Food food))
        {
            Hp = Mathf.Min(maxHp, Hp + food.heal);
            Destroy(other.gameObject);
            return;
        }

        if (other.TryGetComponent<Hazard>(out _))
        {
            if (invincibleTimer > 0f) return;
            ApplyDamage(hitDamage);
            OnHit?.Invoke();
        }
    }

    /// ダメージを受ける（無敵時間を張り直す）。体力0で倒れる
    public void ApplyDamage(float damage)
    {
        if (!Alive) return;
        Hp -= damage;
        invincibleTimer = invincibleTime;
        if (Hp <= 0f)
        {
            Hp = 0f;
            Kill();
        }
    }

    public void Kill()
    {
        if (!Alive) return;
        Alive = false;
        SetVisible(true);
        OnDeath?.Invoke();
    }

    void UpdateInvincible(float dt)
    {
        if (invincibleTimer <= 0f) return;
        invincibleTimer -= dt;
        bool visible = invincibleTimer <= 0f || Mathf.Repeat(invincibleTimer * 12f, 1f) > 0.5f;
        SetVisible(visible);
    }

    void SetVisible(bool v)
    {
        foreach (var r in renderers) if (r) r.enabled = v;
    }
}
