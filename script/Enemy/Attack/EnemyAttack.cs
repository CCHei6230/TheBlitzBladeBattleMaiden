using Unity.Mathematics;
using UnityEngine;

// 敵の通常攻撃オブジェクトの基底クラス。
// Projectiles を継承した発射物であり、iDamageable インターフェースを実装してプレイヤーにダメージを与える。
public class EnemyAttack : Projectiles, iDamageable
{
    // ダメージ量
    [SerializeField] int damage = 10;

    protected virtual void Start()
    {
        Damage = damage;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //　Groundに当たったた際の処理。エフェクトを再生して自身を削除する。
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(Instantiate(destroyEffect, transform.position, quaternion.identity), 1);
            Destroy(gameObject);
        }
    }

    // プレイヤーにパリィされた際の処理。エフェクトを再生して自身を削除する。
    public virtual void BeParry(PlayerProperty playerProperty)
    {
        if (gameObject)
        {
            Destroy(Instantiate(destroyEffect, transform.position, quaternion.identity), 1);
            Destroy(gameObject);
        }
    }

    // プレイヤーに与えるダメージ量（iDamageable インターフェースの実装）
    public int Damage { get; set; }
    // この攻撃を生成したオブジェクトの Transform（iDamageable インターフェースの実装）
    public Transform Spawner { get; set; }
    // この攻撃オブジェクト自身の Transform（iDamageable インターフェースの実装）
    public Transform Transform { get => transform; }
}
