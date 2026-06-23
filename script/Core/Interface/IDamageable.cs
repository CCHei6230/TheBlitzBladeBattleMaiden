using UnityEngine;

// プレイヤーにダメージを与えられる攻撃オブジェクトであることを示すインターフェース。
// EnemyAttack およびその派生クラスが実装している。
public interface IDamageable
{
    // プレイヤーにパリィされた際に呼び出される処理
    public void BeParry(PlayerProperty playerProperty);
    // このオブジェクトがプレイヤーに与えるダメージ量
    public int Damage { get; set; }
    // この攻撃を生成した敵オブジェクトの Transform
    public Transform Spawner { get; set; }
    // この攻撃オブジェクト自身の Transform
    public Transform Transform { get; }
}
