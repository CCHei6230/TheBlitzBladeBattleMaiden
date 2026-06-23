using UnityEngine;

// Trigger() メソッドで起動・中止を切り替えられるオブジェクトの基底クラス。
// デフォルトの実装では gameObject の Active 状態を反転する。
// BarrierObject・EnemyGroupManager・AnchorObject など、外部から制御可能なオブジェクトはこのクラスを継承する。
public class TriggerableObject : MonoBehaviour
{
    // 起動・中止の切り替え処理。デフォルトでは gameObject の Active 状態を反転する。
    // 派生クラスでオーバーライドすることで独自の起動・中止処理を実装できる。
    public virtual void Trigger()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
