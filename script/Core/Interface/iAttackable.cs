using UnityEngine;

// プレイヤーが攻撃できる対象であることを示すインターフェース。
// このインターフェースを実装したオブジェクトは PlayerAttack の当たり判定に反応する。
// EnemyPropertyBase・EnemyAttackHealItem などが実装している。
public interface IAttackable
{
    // 対象オブジェクトのモデルの Transform
    public Transform ModelTransform { get; }
    // プレイヤーの攻撃が命中した際に呼び出されるダメージ処理
    public void DealDamage(PlayerAttack _playerAttack);
}
