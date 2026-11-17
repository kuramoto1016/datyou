using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 前方に地面・障害物・エサを並べ続ける。アセット不要（プリミティブで生成）。
/// 「エサの隣に障害物」など、ダチョウの本能が裏目に出る配置も混ぜる。
/// </summary>
public class TrackSpawner : MonoBehaviour
{
    public Transform target;
    public float laneWidth = 2f;
    public float rowSpacing = 9f;
    public float spawnAhead = 90f;
    public float despawnBehind = 20f;
    public int safeRows = 3;                  // 最初の数列は何も置かない

    [Header("難易度（距離で上がる）")]
    [Range(0, 1)] public float hazardChanceStart = 0.35f;
    [Range(0, 1)] public float hazardChanceMax = 0.75f;
    public float difficultyRampMeters = 1500f;
    [Range(0, 1)] public float baitChance = 0.35f;     // 罠エサ
    [Range(0, 1)] public float wallChance = 0.3f;      // 跳べない壁の割合

    float nextZ;
    int rowIndex;
    readonly Queue<GameObject> spawned = new Queue<GameObject>();
    Material groundMat, fenceMat, wallMat, foodMat;

    void Start()
    {
        groundMat = MakeMat(new Color(0.85f, 0.75f, 0.55f));
        fenceMat = MakeMat(new Color(0.55f, 0.35f, 0.2f));
        wallMat = MakeMat(new Color(0.35f, 0.35f, 0.4f));
        foodMat = MakeMat(new Color(1f, 0.85f, 0.1f));
        nextZ = target.position.z;
    }

    void Update()
    {
        while (nextZ < target.position.z + spawnAhead)
            SpawnRow();

        while (spawned.Count > 0 && spawned.Peek() != null &&
               spawned.Peek().transform.position.z < target.position.z - despawnBehind)
            Destroy(spawned.Dequeue());
        while (spawned.Count > 0 && spawned.Peek() == null) spawned.Dequeue();
    }

    void SpawnRow()
    {
        float z = nextZ;

        // 地面
        var g = Make(PrimitiveType.Cube, new Vector3(0, -0.5f, z + rowSpacing * 0.5f),
                     new Vector3(laneWidth * 3.2f, 1f, rowSpacing), groundMat);
        Destroy(g.GetComponent<Collider>());

        if (rowIndex++ >= safeRows)
        {
            float t = Mathf.Clamp01(z / difficultyRampMeters);
            float hazardChance = Mathf.Lerp(hazardChanceStart, hazardChanceMax, t);

            // 最低1レーンは必ず空ける
            int openLane = Random.Range(-1, 2);
            for (int lane = -1; lane <= 1; lane++)
            {
                if (lane == openLane || Random.value > hazardChance) continue;
                SpawnHazard(lane, z, Random.value < wallChance);
            }

            // エサ：わざと危ない列の隣に置くことがある
            if (Random.value < baitChance)
            {
                int foodLane = Random.Range(-1, 2);
                SpawnFood(foodLane, z - 3f);        // エサの直後に障害物 → 釣られると当たる
                SpawnHazard(foodLane, z + 1f, false);
            }
            else if (Random.value < 0.3f)
            {
                SpawnFood(openLane, z);             // ご褒美エサ
            }
        }

        nextZ += rowSpacing;
    }

    void SpawnHazard(int lane, float z, bool wall)
    {
        Vector3 size = wall ? new Vector3(laneWidth * 0.9f, 3f, 0.6f) : new Vector3(laneWidth * 0.9f, 0.8f, 0.4f);
        var h = Make(PrimitiveType.Cube, new Vector3(lane * laneWidth, size.y * 0.5f, z), size, wall ? wallMat : fenceMat);
        h.GetComponent<Collider>().isTrigger = true;
        h.AddComponent<Hazard>().low = !wall;
    }

    void SpawnFood(int lane, float z)
    {
        var f = Make(PrimitiveType.Sphere, new Vector3(lane * laneWidth, 0.6f, z), Vector3.one * 0.6f, foodMat);
        f.GetComponent<Collider>().isTrigger = true;
        f.AddComponent<Food>();
    }

    GameObject Make(PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(transform);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        spawned.Enqueue(go);
        return go;
    }

    static Material MakeMat(Color c)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(shader);
        m.color = c;
        return m;
    }
}
