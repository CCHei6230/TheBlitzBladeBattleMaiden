using UnityEngine;

// EnemyAttack の派生クラス。
// プレイヤーに当たった際・パリィされた際・ワープ攻撃された際に、プレイヤーの HP を回復する特殊な攻撃オブジェクト。
// ITaggableObject を実装しており、ワープ攻撃のターゲットにもなる。
public class EnemyAttackHealItem : EnemyAttack, ITaggableObject, IAttackable
{
    // ワープ攻撃を受けた際の処理。プレイヤーの HP を回復してエフェクトを再生し、自身を削除する。
    public void BeSlashed(PlayerProperty playerProperty)
    {
        // プレイヤーの HP を 10 回復する
        playerProperty.playerHPManager.HPIncreased(10);
        Destroy(Instantiate(destroyEffect, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }

    // ワープ攻撃命中後の HP を返す処理。
    // この攻撃オブジェクトは HP の概念がないため、Rigidbody を削除して移動を止め、999 を返す。
    public int HPAfterBeSlashed(int _atkLv)
    {
        Destroy(RB);
        return 999;
    }

    void OnTriggerEnter(Collider _other)
    {
        //　Groundに当たったた際の処理。エフェクトを再生して自身を削除する。
        if (_other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(Instantiate(destroyEffect, transform.position, Quaternion.identity), 1);
            Destroy(gameObject);
        }
    }

    // プレイヤーにパリィされた際の処理。プレイヤーの HP を回復してエフェクトを再生し、自身を削除する。
    public override void BeParry(PlayerProperty playerProperty)
    {
        // プレイヤーの HP を 10 回復する
        playerProperty.playerHPManager.HPIncreased(10);
        Destroy(Instantiate(destroyEffect, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }

    // このオブジェクトのモデルの Transform を返すプロパティ（ITaggableObject インターフェースの実装）
    public Transform ModleTransform { get => transform; }

    // プレイヤーの攻撃が命中した際の処理（IAttackable インターフェースの実装）。
    // プレイヤーの HP を回復し、ワープ攻撃のタグリストから自身を削除して、自身を破棄する。
    public void DealDamage(PlayerAttack _playerAttack)
    {
        // プレイヤーの HP を 10 回復する
        _playerAttack.GetComponentInParent<PlayerHPManager>().HPIncreased(10);
        _playerAttack.GetComponentInParent<PlayerWarpAttackManager>().RemoveTagTransform(transform);
        Destroy(gameObject);
    }
}
