using UnityEngine;

// 背景オブジェクト用のクラス。
// 指定した速度でターゲットオブジェクトを毎フレーム回転させ続ける。
// 起動時にランダムな初期回転を適用することで、複数配置した際の単調さを防ぐ。
public class ObjectRotate : MonoBehaviour
{
    // 各軸方向の回転速度（度/秒）
    [SerializeField] Vector3 rotationSpeed;
    // 回転させる対象の Transform
    [SerializeField] Transform target;

    void Start()
    {
        // 起動時にランダムな初期回転を適用する（-360〜360 度のランダムオフセット）
        target.rotation = Quaternion.Euler(target.rotation.eulerAngles + rotationSpeed * Random.Range(-360f, 360f));
    }

    void FixedUpdate()
    {
        // 毎 FixedUpdate フレームで回転速度に応じてオブジェクトを回転させる
        target.rotation = Quaternion.Euler(target.rotation.eulerAngles + rotationSpeed * Time.fixedDeltaTime);
    }
}
