using UnityEngine;

// プレイヤーのワープ攻撃によるタグ付けの対象となれるオブジェクトを示すインターフェース。
// このインターフェースを実装したオブジェクトは集中モード中に検知・タグ付けされ、ワープ攻撃の対象となる。
// EnemyBehavior・BossBehavior・AnchorObject・EnemyAttackHealItem が実装している。
public interface ITaggableObject
{
    // ワープ攻撃が命中した際に呼び出される処理
    public void BeSlashed(PlayerProperty playProperty);
    // ワープ攻撃命中後のダメージ適用前の HP を返す処理。
    public int HPAfterBeSlashed(int _atkLv);
    // 対象オブジェクトのモデルの Transform
    public Transform ModleTransform { get; }
}
