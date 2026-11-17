using UnityEngine;

/// 当たるとダメージ。low=true ならジャンプで越えられる柵、false なら跳べない壁。
public class Hazard : MonoBehaviour
{
    public bool low = true;
}

/// エサ。ダチョウが勝手に寄っていく。食べると少し回復。
public class Food : MonoBehaviour
{
    public float heal = 10f;

    void Update() => transform.Rotate(0f, 120f * Time.deltaTime, 0f);
}
