using UnityEngine;

/// <summary>
/// 見た目（プリミティブの組み合わせ）を作る場所。アセットに差し替えるときはここを直す。
/// ※ダチョウの見た目はまだ GameBootstrap にある。いずれここへ移す（TODO.md）
/// </summary>
public static class Visuals
{
    /// URP でも Built-in でも使えるマテリアル
    public static Material Mat(Color c)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = shader != null ? new Material(shader) : new Material(DefaultMaterial());
        m.color = c;
        return m;
    }

    static Material defaultMat;

    /// どちらのシェーダーも見つからないとき用：プリミティブに最初から付いているマテリアルを借りる
    static Material DefaultMaterial()
    {
        if (defaultMat == null)
        {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            defaultMat = tmp.GetComponent<Renderer>().sharedMaterial;
            Object.DestroyImmediate(tmp);
        }
        return defaultMat;
    }

    /// 当たり判定のない見た目だけのプリミティブ
    public static GameObject Part(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        // 見た目だけなので当たり判定は即座に消す（Destroyだと1フレーム残ってダチョウのトリガーに触れうる）
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    /// <summary>
    /// 追っ手のドローン。黒い胴体＋4つのローター＋赤い目＋捕獲ビーム（最初は非表示）。
    /// </summary>
    public static GameObject CreateDrone(out Transform[] rotors, out GameObject beam)
    {
        var root = new GameObject("PursuerDrone");
        var bodyMat = Mat(new Color(0.12f, 0.12f, 0.15f));
        var armMat = Mat(new Color(0.3f, 0.3f, 0.35f));
        var eyeMat = Mat(new Color(1f, 0.1f, 0.1f));
        var beamMat = Mat(new Color(1f, 0.3f, 0.3f));

        Part(PrimitiveType.Sphere, root.transform, Vector3.zero, new Vector3(1.2f, 0.45f, 1.2f), bodyMat);
        // 赤い目はダチョウの方（前＝+Z）を向く
        Part(PrimitiveType.Sphere, root.transform, new Vector3(0f, -0.05f, 0.55f), Vector3.one * 0.25f, eyeMat);

        rotors = new Transform[4];
        int i = 0;
        for (int sx = -1; sx <= 1; sx += 2)
        for (int sz = -1; sz <= 1; sz += 2)
        {
            Vector3 corner = new Vector3(sx * 0.75f, 0.1f, sz * 0.75f);
            var arm = Part(PrimitiveType.Cube, root.transform, corner * 0.5f, new Vector3(0.1f, 0.08f, 0.1f), armMat);
            arm.transform.localRotation = Quaternion.LookRotation(corner);
            arm.transform.localScale = new Vector3(0.1f, 0.08f, corner.magnitude);
            var rotor = Part(PrimitiveType.Cube, root.transform, corner + Vector3.up * 0.1f, new Vector3(0.8f, 0.03f, 0.1f), armMat);
            rotors[i++] = rotor.transform;
        }

        // 捕獲ビーム：真下に伸びる細い円柱（Cylinder は高さ2なので scale.y=1.2 で長さ2.4）
        beam = Part(PrimitiveType.Cylinder, root.transform, new Vector3(0f, -1.4f, 0.3f), new Vector3(0.35f, 1.2f, 0.35f), beamMat);
        beam.SetActive(false);

        return root;
    }
}
