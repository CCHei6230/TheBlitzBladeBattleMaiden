using UnityEngine;
using UnityEngine.Serialization;

// プレイヤーの攻撃当たり判定を管理するクラス。
// OnTriggerEnter で IAttackable を実装したオブジェクトに当たった際の処理を行う。
public class PlayerAttack : MonoBehaviour
{
    // プレイヤーの攻撃力（攻撃レベルやスキルによって動的に変更される）
    public int damage = 10;
    // プレイヤーのパラメータ・参照を保持するプロパティクラス
    public PlayerProperty playerProperty;
    // この攻撃で攻撃レベルを上昇できるかどうかのフラグ
    public bool canIncreaseAttackLv = true;

    // 敵を倒した際に呼び出される処理。攻撃レベルの上昇が許可されている場合に上昇処理を実行する。
    public void KilledEnemy()
    {
        if (canIncreaseAttackLv)
        {
            playerProperty.PlayerAttackLvIncreased();
        }
    }

    // 当たり判定のコールバック。IAttackable を実装したオブジェクトに触れた際に呼び出される。
    void OnTriggerEnter(Collider _collider)
    {
        // 接触したオブジェクトが IAttackable を実装しているか確認する
        _collider.TryGetComponent(out IAttackable tmp_attackable);
        if (tmp_attackable != null)
        {
            // ダメージを与える
            tmp_attackable.DealDamage(this);
            // ヒットストップを実行する（スローモーション + 画面振動）
            HitStopManager.instance.HitStop(0.85f, 2);
            // ヒット効果音を再生する
            AudioClipManager.instance.PlayAudioOneShot("SFX_HitEnemy");
            // コントローラーを振動させる
            InputHaptic.instance.Haptic(0.25f, 0.25f, 2);
            // 集中ゲージを 5 増加させる
            playerProperty.focusCount += 5;
            // ヒットエフェクトを生成
            Destroy(Instantiate(playerProperty.hitEffect, tmp_attackable.ModelTransform.position, Random.rotation), 2f);
        }
    }
}
