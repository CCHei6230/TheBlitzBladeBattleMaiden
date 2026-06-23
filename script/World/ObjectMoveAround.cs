using Unity.Mathematics.Geometry;
using UnityEngine;

// 背景オブジェクト用のクラス。
// Sin 波を使ってターゲットオブジェクトを指定した速度・範囲でゆらゆらと移動させる。
public class ObjectMoveAround : MonoBehaviour
{
    // 移動させる対象の Transform
    [SerializeField] Transform target;
    // 移動の起点となる初期位置
    Vector3 startPos;
    // 各軸方向の移動速度（Sin 波の周波数に相当する）
    [SerializeField] Vector3 speed;
    // 各軸方向の移動範囲（Sin 波の振幅に相当する）
    [SerializeField] Vector3 range;

    void Start()
    {
        // 起点位置を記録する
        startPos = target.position;
        // 起動時にランダムな位相オフセットを適用して初期位置をずらす
        target.position = startPos + new Vector3(
            Mathf.Sin(speed.x * Random.Range(1, 10)) * range.x,
            Mathf.Sin(speed.y * Random.Range(1, 10)) * range.y,
            Mathf.Sin(speed.z * Random.Range(1, 10)) * range.z
        ) * Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        // 経過時間に基づいた Sin 波でターゲットを移動させる
        target.position = startPos + new Vector3(
            Mathf.Sin(speed.x * Time.fixedTime) * range.x,
            Mathf.Sin(speed.y * Time.fixedTime) * range.y,
            Mathf.Sin(speed.z * Time.fixedTime) * range.z
        ) * Time.fixedDeltaTime;
    }
}
