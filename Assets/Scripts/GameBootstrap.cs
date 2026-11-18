using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 空のシーンに置くだけで全部組み立てるブートストラップ＋HUD。
/// 空のGameObjectにこれ1つ付けて再生すればOK。
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    OstrichController ostrich;
    PsychicController psychic;
    PursuerDrone pursuer;
    Camera cam;
    bool gameOver;
    float best;

    const float CamHeight = 5f, CamBack = 8f;

    void Start()
    {
        best = PlayerPrefs.GetFloat("best", 0f);

        // ダチョウ（仮：カプセル＋首）
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Ostrich";
        body.transform.position = new Vector3(0, 1f, 0);
        body.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
        var col = body.GetComponent<CapsuleCollider>();
        col.isTrigger = true;
        var neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(neck.GetComponent<Collider>());
        neck.transform.SetParent(body.transform);
        neck.transform.localPosition = new Vector3(0, 1.2f, 0.3f);
        neck.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
        neck.GetComponent<Renderer>().material.color = new Color(0.9f, 0.7f, 0.6f);

        body.AddComponent<Rigidbody>();
        ostrich = body.AddComponent<OstrichController>();
        ostrich.OnDeath += HandleDeath;

        // 主人公（今は見えない念の主）
        psychic = new GameObject("Psychic").AddComponent<PsychicController>();
        psychic.ostrich = ostrich;

        // コース
        var spawner = new GameObject("Track").AddComponent<TrackSpawner>();
        spawner.target = body.transform;

        // 追っ手（後ろから迫るドローン）
        pursuer = new GameObject("Pursuer").AddComponent<PursuerDrone>();
        pursuer.ostrich = ostrich;

        // カメラ・ライト
        cam = Camera.main;
        if (cam == null) cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.backgroundColor = new Color(0.6f, 0.8f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        if (FindObjectOfType<Light>() == null)
        {
            var l = new GameObject("Sun").AddComponent<Light>();
            l.type = LightType.Directional;
            l.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }

    void LateUpdate()
    {
        if (ostrich == null) return;
        Vector3 p = ostrich.transform.position;
        cam.transform.position = new Vector3(p.x * 0.5f, CamHeight, p.z - CamBack);
        cam.transform.LookAt(new Vector3(p.x * 0.5f, 1f, p.z + 6f));

        if (gameOver && (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(0)))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void HandleDeath()
    {
        gameOver = true;
        if (ostrich.Distance > best)
        {
            best = ostrich.Distance;
            PlayerPrefs.SetFloat("best", best);
        }
    }

    // ---------- HUD（OnGUIで手抜き。本番はCanvasに置き換え） ----------

    void OnGUI()
    {
        if (ostrich == null) return;
        float s = Screen.height / 720f;
        var label = new GUIStyle(GUI.skin.label) { fontSize = (int)(28 * s), fontStyle = FontStyle.Bold };
        label.normal.textColor = Color.black;

        GUI.Label(new Rect(20 * s, 15 * s, 600 * s, 40 * s), $"{ostrich.Distance:0} m   (BEST {best:0} m)", label);

        Bar(new Rect(20 * s, 60 * s, 300 * s, 22 * s), ostrich.Hp / ostrich.maxHp, new Color(0.3f, 0.85f, 0.3f), "ダチョウ体力", s);
        Bar(new Rect(20 * s, 92 * s, 300 * s, 22 * s), psychic.Baldness01, new Color(0.95f, 0.6f, 0.5f), "頭ダチョウ度", s);

        if (pursuer != null)
        {
            Color gapColor = pursuer.InDanger ? new Color(0.95f, 0.25f, 0.25f) : new Color(0.5f, 0.6f, 0.95f);
            Bar(new Rect(20 * s, 124 * s, 300 * s, 22 * s), pursuer.Gap01, gapColor, $"追っ手との距離 {pursuer.Gap:0}m", s);
        }

        string mode = ostrich.IsCommanded ? $"命令中… 忘れるまで {ostrich.CommandTimeLeft:0.0}s" : "本能で走ってる";
        GUI.Label(new Rect(20 * s, 156 * s, 600 * s, 40 * s), mode, label);

        if (!gameOver && pursuer != null && pursuer.InDanger)
        {
            var warn = new GUIStyle(label) { fontSize = (int)(40 * s), alignment = TextAnchor.MiddleCenter };
            warn.normal.textColor = new Color(0.9f, 0.1f, 0.1f);
            // 点滅
            if (Mathf.Repeat(Time.time * 3f, 1f) > 0.35f)
                GUI.Label(new Rect(0, Screen.height * 0.18f, Screen.width, 60 * s), "ドローン接近中！", warn);
        }

        if (gameOver)
        {
            var big = new GUIStyle(label) { fontSize = (int)(64 * s), alignment = TextAnchor.MiddleCenter };
            var mid = new GUIStyle(label) { fontSize = (int)(30 * s), alignment = TextAnchor.MiddleCenter };
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            string title = psychic.IsOstrichHead ? "クエ？" : "捕まった…";
            string sub = psychic.IsOstrichHead ? "念を使いすぎて頭がダチョウになった"
                       : (pursuer != null && pursuer.KilledOstrich) ? "ドローンの捕獲ビームでダチョウが倒れた"
                       : "ダチョウの体力が尽きた";
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 90 * s), title, big);
            GUI.Label(new Rect(0, Screen.height * 0.35f + 90 * s, Screen.width, 50 * s), sub, mid);
            GUI.Label(new Rect(0, Screen.height * 0.35f + 150 * s, Screen.width, 50 * s), $"{ostrich.Distance:0} m 逃げた　R / タップでリトライ", mid);
        }
    }

    static void Bar(Rect r, float v, Color c, string name, float s)
    {
        var old = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.4f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = c;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(v), r.height), Texture2D.whiteTexture);
        GUI.color = old;
        var st = new GUIStyle(GUI.skin.label) { fontSize = (int)(16 * s) };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(r.x + 6 * s, r.y, r.width, r.height), name, st);
    }
}
