using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using Random = UnityEngine.Random;

// エネミーステートEnum
public enum EnemyState
{
    spawn,
    standby,
    attack,
    approach,
    damage
}

// 出現ステート
public class EnemyFlyingEyeBallState_SpawnState : EnemyStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.modelObject.SetActive(false);
        EnemyProperty.GetComponent<Collider>().enabled = false;
        // Rigidbodyが存在する場合、速度を0に
        if (EnemyProperty.RB)
        {
            EnemyProperty.RB.linearVelocity = Vector3.zero;
        }
        GameObject.Destroy(   GameObject.Instantiate(EnemyProperty.spawnEffectPrefab,EnemyProperty.modelObject.transform.position,Quaternion.identity),5f);
        AudioClipManager.instance.PlayAudioOneShot("SFX_EnemySpawn");
 	}

    public override StateBase UpdateEvent()
    {
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        EnemyProperty.waitToSpawnCount++;
        if (EnemyProperty.RB)
        {
            // 速度を0に固定
            EnemyProperty.RB.linearVelocity = Vector3.zero;
        }

        // カウントが120に達した場合
        if (EnemyProperty.waitToSpawnCount == 120)
        {
            EnemyProperty.modelObject.SetActive(true);
            // standby stateに遷移
            nextState = EnemyProperty.stateDictionary[EnemyState.standby];
        }

        return nextState;
    }

    public override void ExitEvent()
    {
        EnemyProperty.GetComponent<Collider>().enabled = true;
    }
}

// 待機ステート
public class EnemyFlyingEyeBallState_StandbyState : EnemyStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
    }

    public override StateBase UpdateEvent()
    {
        // HPが0以下の場合、死亡処理
        if (EnemyProperty.hp <= 0)
        {
            HitStopManager.instance.HitStop(0.5f,2,true);
            GameObject.Destroy(
                GameObject.Instantiate(EnemyProperty.deathEffectPrefab,
                    EnemyProperty.transform.position,quaternion.identity),2f);
            EnemyProperty.playerAttack.KilledEnemy();
            EnemyProperty.modelObject.transform.DOKill();
            EnemyProperty.groupManager.RemoveEnemy(EnemyProperty.gameObject);
            EnemyProperty.playerAttack.GetComponentInParent<PlayerWarpAttackManager>().RemoveTagTransform(EnemyProperty.transform);
            GameObject.Destroy(EnemyProperty.gameObject);
        }
        // プレイヤーとの距離に応じてステート遷移
        nextState = EnemySharedMethods.PlayerInDetectDistance(EnemyProperty, this);
        return nextState;
    }
    public override StateBase FixedUpdateEvent(){ return nextState; }
}

// 追跡ステート
public class EnemyFlyingEyeBallState_ApproachState : EnemyStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // モデルのDOComplete
        EnemyProperty.modelObject.transform.DOComplete();
    }

    public override StateBase UpdateEvent()
    {
        // HPが0以下の場合、死亡処理
        if (EnemyProperty.hp <= 0)
        {
            HitStopManager.instance.HitStop(0.5f,2,true);
            GameObject.Destroy(
                GameObject.Instantiate(EnemyProperty.deathEffectPrefab,
                    EnemyProperty.transform.position,quaternion.identity),2f);
            EnemyProperty.playerAttack.KilledEnemy();
            EnemyProperty.modelObject.transform.DOKill();
            EnemyProperty.groupManager.RemoveEnemy(EnemyProperty.gameObject);
            EnemyProperty.playerAttack.GetComponentInParent<PlayerWarpAttackManager>().RemoveTagTransform(EnemyProperty.transform);

            GameObject.Destroy(EnemyProperty.gameObject);
        }
        // プレイヤーとの距離に応じてステート遷移
        nextState = EnemySharedMethods.PlayerInDetectDistance(EnemyProperty, this);
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        if (EnemyProperty.RB)
        {
            EnemyProperty.RB.linearVelocity = (
                (EnemyProperty.playerTransform.position - EnemyProperty.modelObject.transform.position).normalized*
                EnemyProperty.speed * Time.fixedDeltaTime);
        }
        // プレイヤーの方向に向く
        EnemyProperty.modelObject.transform
            .DOLookAt(EnemyProperty.playerTransform.position, 0.01f);
        return nextState;
    }

    public override void ExitEvent()
    {
        // 速度を0に固定
        if (EnemyProperty.RB)
        EnemyProperty.RB.linearVelocity = Vector3.zero;
    }
    public override void TriggerEnterEvent(Collider other)
    {
        // モデルのDOComplete
        EnemyProperty.modelObject.transform.DOComplete();
    }

    public override void TriggerStayEvent(Collider other)
    {
        // モデルのDOComplete
        EnemyProperty.modelObject.transform.DOComplete();
    }

}

// 攻撃ステート
public class EnemyFlyingEyeBallState_AttackState : EnemyStateBase
{

    public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.attackDuration   = 1;
        // モデルのDOComplete
        EnemyProperty.modelObject.transform.DOComplete();
    }

    public override StateBase UpdateEvent()
    {
        // HPが0以下の場合、死亡処理
        if (EnemyProperty.hp <= 0)
        {
            HitStopManager.instance.HitStop(0.5f,2,true);
            GameObject.Destroy(
                GameObject.Instantiate(EnemyProperty.deathEffectPrefab,
                    EnemyProperty.transform.position,quaternion.identity),2f);
            EnemyProperty.playerAttack.KilledEnemy();
            EnemyProperty.modelObject.transform.DOKill();
            EnemyProperty.groupManager.RemoveEnemy(EnemyProperty.gameObject);
            EnemyProperty.playerAttack.GetComponentInParent<PlayerWarpAttackManager>().RemoveTagTransform(EnemyProperty.transform);
            GameObject.Destroy(EnemyProperty.gameObject);
        }
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        // 攻撃処理
        if ( EnemyProperty.attackDuration  > 0)
        {
            if (EnemyProperty.RB)
            {
                EnemyProperty.attackDuration --;
                EnemyProperty.RB.linearVelocity = (
                    (EnemyProperty.playerTransform.position - EnemyProperty.modelObject.transform.position).normalized*
                    EnemyProperty.speed * 0.8f * Time.fixedDeltaTime);

                // プレイヤーの方向に向く
                EnemyProperty.modelObject.transform
                    .DOLookAt(EnemyProperty.playerTransform.position, 0.75f);

                // チャージ完了タイミング
                if ( EnemyProperty.attackDuration  == 0)
                {
                    AudioClipManager.instance.PlayAudioOneShot("SFX_EnemyAttack");
                    GameObject.Destroy(GameObject.Instantiate(EnemyProperty. chargeEndEffectPrefab,EnemyProperty.modelObject.transform.position,quaternion.identity),2f);

                    // プレイヤーの方を向いて弾を発射
                    EnemyProperty.modelObject.transform
                        .DOLookAt(EnemyProperty.playerTransform.position, 0.5f)
                        .OnComplete(
                            () =>
                            {
                                // 攻撃距離を調整
                                EnemyProperty.distanceToAttack *= 0.8f;
                                if (EnemyProperty.distanceToAttack < EnemyProperty.minimumDistanceToAttack)
                                {
                                    EnemyProperty.distanceToAttack *= 1.5f;
                                }

                                EnemyProperty.attackDuration  = EnemyProperty.attackDurationMax;

                                GameObject tmp_attack = null;
                                if (Random.Range(1, 100) / 100f < 0.1f)
                                {
                                    tmp_attack =
                                    GameObject.Instantiate(EnemyProperty. attackHealItemPrefab,EnemyProperty.modelObject.transform.position,EnemyProperty.modelObject.transform.rotation);
                                }
                                else
                                {
                                    tmp_attack=    GameObject.Instantiate(EnemyProperty. attackPrefab,EnemyProperty.modelObject.transform.position,EnemyProperty.modelObject.transform.rotation);
                                }

                                tmp_attack.GetComponentInChildren<iDamageable>().Spawner = EnemyProperty.ModleTransform;
                                // プレイヤーとの距離に応じてステート遷移
                                nextState = EnemySharedMethods.PlayerInDetectDistance(EnemyProperty, this);
                            }
                        );
                }
            }
        }
        return nextState;
    }
}

// ダメージステート
public class EnemyFlyingEyeBallState_DamageState : EnemyStateBase
{
   public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.damageCount = EnemyProperty.damageCountMax;
        // 速度を0に固定
        if (EnemyProperty.RB)
        {
            EnemyProperty.RB.linearVelocity = Vector3.zero;
        }
        EnemyProperty.hp -= EnemyProperty.playerAttack.damage;
        // HPが0以下の場合、死亡処理
        if (EnemyProperty.hp <= 0)
        {
            HitStopManager.instance.HitStop(0.5f,2,true);
            GameObject.Destroy(
                GameObject.Instantiate(EnemyProperty.deathEffectPrefab,
                    EnemyProperty.transform.position,quaternion.identity),2f);
            EnemyProperty.playerAttack.KilledEnemy();
            EnemyProperty.modelObject.transform.DOKill();
            EnemyProperty.groupManager.RemoveEnemy(EnemyProperty.gameObject);
            GameObject.Destroy(EnemyProperty.gameObject);
        }
    }

    public override StateBase UpdateEvent()
    {
        // ダメージカウントが終了した場合
        if (EnemyProperty.damageCount <= 0)
        {
            // standby stateに遷移
            nextState = EnemyProperty.stateDictionary[EnemyState.standby];
        }
        return nextState;
    }
    public override StateBase FixedUpdateEvent()
    {
        EnemyProperty.damageCount--;
        return this;
    }
    public override void ExitEvent()
    {
        EnemyProperty.playerAttack = null;
    }
}