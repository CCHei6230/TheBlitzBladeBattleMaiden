using System;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using Random = UnityEngine.Random;

// 出現ステート
public class EnemyGroundEyeBallState_SpawnState : EnemyStateBase
{

    public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.modelObject.SetActive(false);
        EnemyProperty.GetComponent<Collider>().enabled = false;

        if (EnemyProperty.RB)
        {
            // 速度を0に固定
            EnemyProperty.RB.linearVelocity = Vector3.zero;
            EnemyProperty.RB.useGravity = false;
        }

        GameObject.Destroy(   GameObject.Instantiate(EnemyProperty.spawnEffectPrefab,EnemyProperty.modelObject.transform.position,Quaternion.identity),5f);
        AudioClipManager.instance.PlayAudioOneShot("SFX_EnemySpawn");
 	}

    public override StateBase UpdateEvent()
    {
        // プレイヤーの方向に向く
        EnemyProperty.modelObject.transform
            .DOLookAt(EnemyProperty.playerTransform.position, 0.01f,AxisConstraint.Y);
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

        // カウントが105に達した場合
        if (EnemyProperty.waitToSpawnCount == 105)
        {
            EnemyProperty.modelObject.SetActive(true);
            EnemyProperty.animator.CrossFade(EnemyProperty.spawnAnimation.name , 0);
        }
        // カウントが140に達した場合
        if (EnemyProperty.waitToSpawnCount == 140)
        {
            // standby stateに遷移
            nextState = EnemyProperty.stateDictionary[EnemyState.standby];
        }

        return nextState;
    }

    public override void ExitEvent()
    {
        EnemyProperty.GetComponent<Collider>().enabled = true;
        if (EnemyProperty.RB)
        {
            EnemyProperty.RB.useGravity = true;
        }
    }

}

// 待機ステート
public class EnemyGroundEyeBallState_StandbyState : EnemyStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.animator.CrossFade(EnemyProperty.idleAnimation.name,0.1f);
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
        if (EnemyProperty.RB)
        {
            // 水平速度を0に固定
            EnemyProperty.RB.linearVelocity = new Vector3(0, EnemyProperty.RB.linearVelocity.y, 0);
        }

        // 着地判定
        if (Physics.CheckSphere(EnemyProperty.groundCheckTransform.position, EnemyProperty.groundCheckDistance,
                EnemyProperty.groundLayer))
        {
            EnemyProperty.animator.CrossFade(EnemyProperty.idleAnimation.name,0.1f);
        }
        else
        {
            EnemyProperty.animator.CrossFade(EnemyProperty.fallAnimation.name,0.1f);
        }

        // プレイヤーとの距離に応じてステート遷移
        nextState = EnemySharedMethods.PlayerInDetectDistance(EnemyProperty, this);
        return nextState;
    }
    public override StateBase FixedUpdateEvent(){ return nextState; }
}

// 追跡ステート
public class EnemyGroundEyeBallState_ApproachState : EnemyStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.animator.CrossFade(EnemyProperty.runningAnimation.name,0.1f);
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

        // 着地判定
        if (Physics.CheckSphere(EnemyProperty.groundCheckTransform.position, EnemyProperty.groundCheckDistance,
                EnemyProperty.groundLayer))
        {
            EnemyProperty.animator.Play(EnemyProperty.runningAnimation.name);
        }
        else
        {
            EnemyProperty.animator.Play(EnemyProperty.fallAnimation.name);
        }

        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        if (EnemyProperty.RB)
        {
            Vector3 tmp_vec = EnemyProperty.modelObject.transform.forward * EnemyProperty.speed * Time.fixedDeltaTime;
            EnemyProperty.RB.linearVelocity = new Vector3(tmp_vec.x,EnemyProperty.RB.linearVelocity.y,tmp_vec.z);
        }

        // プレイヤーの方向に向く
        EnemyProperty.modelObject.transform
            .DOLookAt(EnemyProperty.playerTransform.position, 0.01f,AxisConstraint.Y);

        return nextState;
    }

    public override void ExitEvent()
    {
        if (EnemyProperty.RB)
        {
            // 速度を0に固定
            EnemyProperty.RB.linearVelocity = Vector3.zero;
        }
    }
}

// 攻撃ステート
public class EnemyGroundEyeBallState_AttackState : EnemyStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        EnemyProperty.attackDuration  = 1;
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
        if (EnemyProperty.attackDuration > 0)
        {
            if (EnemyProperty.RB)
            {
                EnemyProperty.attackDuration--;
                // プレイヤーの方向に向く
                EnemyProperty.modelObject.transform
                    .DOLookAt(EnemyProperty.playerTransform.position, 0.75f,AxisConstraint.Y);

                // チャージ完了タイミング
                if (EnemyProperty.attackDuration == 0)
                {
                    AudioClipManager.instance.PlayAudioOneShot("SFX_EnemyAttack");
                    GameObject.Destroy(GameObject.Instantiate(EnemyProperty. chargeEndEffectPrefab,EnemyProperty.modelObject.transform.position,quaternion.identity),2f);

                    // プレイヤーの方を向いて弾を発射
                    EnemyProperty.modelObject.transform
                        .DOLookAt(EnemyProperty.playerTransform.position, 0.5f,AxisConstraint.Y)
                        .OnStart(
                            () => {
                                EnemyProperty.animator.CrossFade(EnemyProperty.shootAnimation.name,0.1f);
                                // 速度を0に固定
                                EnemyProperty.RB.linearVelocity = Vector3.zero;
                            }
                        )
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
                                tmp_attack.GetComponentInChildren<IDamageable>().Spawner = EnemyProperty.ModleTransform;
                                // プレイヤーとの距離に応じてステート遷移
                                nextState = EnemySharedMethods.PlayerInDetectDistance(EnemyProperty, this);
                                // アイドルアニメーションに遷移
                                EnemyProperty.animator.CrossFade(EnemyProperty.idleAnimation.name,0.3f);
                            }
                        );
                }
            }
        }
        return nextState;
    }
}

// ダメージステート
public class EnemyGroundEyeBallState_DamageState : EnemyStateBase
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
            EnemyProperty.playerAttack.GetComponentInParent<PlayerWarpAttackManager>().RemoveTagTransform(EnemyProperty.transform);
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