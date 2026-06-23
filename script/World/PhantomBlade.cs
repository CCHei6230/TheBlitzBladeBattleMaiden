using UnityEngine;
using DG.Tweening;

// ターゲットをタグする際に発射される幻影剣オブジェクト。
// Projectiles を継承した発射物であり、敵に当たった場合はタグし、地面に当たった場合はそのまま消滅する。
public class PhantomBlade : Projectiles
{
    // この幻影剣を生成したワープ攻撃マネージャーの参照
    public PlayerWarpAttackManager manager;
    // 追尾するターゲットの Transform
    [SerializeField] private Transform target;

    // 発射時の初期データを設定する処理。
    // _speed    : 速度
    // _manager  : ワープ攻撃マネージャーの参照
    // _position : 発射位置
    // _target   : 追尾するターゲットの Transform（null の場合は直進）
    public void SetData(float _speed, PlayerWarpAttackManager _manager, Vector3 _position, Transform _target = null)
    {
        speed = _speed;
        transform.position = _position;
        manager = _manager;
        target = _target;

        // ターゲットが存在する場合はその方向に向く
        if (target)
        {
            transform.LookAt(target, Vector3.up);
        }
    }

    private void FixedUpdate()
    {
        // 処理順序の影響で方向がずれる場合があるため、FixedUpdate の最初に再度方向を設定する
        if (target)
        {
            transform.LookAt(target, Vector3.up);
            target = null;
        }

        base.FixedUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 地面に当たった場合の処理
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(RB);
            Destroy(Instantiate(destroyEffect, transform.position, Quaternion.identity), 1);
            Destroy(GetComponentInChildren<Collider>());
            GetComponentInChildren<ParticleSystem>().Stop();
            renderer.material.DOFloat(0, "_Disslove", 1.25f);
            Destroy(gameObject, 1.5f);
        }

        // 敵に当たった場合の処理
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // コントローラーを軽く振動させる
            InputHaptic.instance.Haptic(0.1f, 0.1f, 2);
            // 当たった敵をワープ攻撃のターゲットとしてタグ
            manager.TagObject(other.transform);
            Destroy(RB);
            Destroy(Instantiate(destroyEffect, transform.position, Quaternion.identity), 1);
            Destroy(GetComponentInChildren<Collider>());
            GetComponentInChildren<ParticleSystem>().Stop();
            renderer.material.DOFloat(0, "_Disslove", 1.25f);
            Destroy(gameObject, 1.5f);
        }
    }
}
