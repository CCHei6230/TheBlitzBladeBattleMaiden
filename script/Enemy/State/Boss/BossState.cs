using DG.Tweening;
using UnityEngine;

// ボスステートEnum
public enum BossState
{
    Standby,
    Shielding,
    WaitToMove,
    Moving,
}

// ボスステートの基本クラス
public  class BossStateBase : StateBase
{
    // ボスのプロパティクラス
    static protected BossProperty BossProperty;
    // ボスのプロパティを設定
    public void SetProperty(BossProperty bossProperty)
    {
        if (BossProperty != null) return;
        BossProperty = bossProperty;
    }
}

// スタンバイステート（出現演出）
public class BossState_StandbyState : BossStateBase
{

    public override void StateEnterEvent()
    {
		 	nextState = this;
            BossProperty.modelObject.SetActive(false);
            BossProperty.GetComponent<Collider>().enabled = false;
            GameObject.Destroy(   GameObject.Instantiate(BossProperty.spawnEffectPrefab,BossProperty.modelObject.transform.position,Quaternion.identity),5f);
            AudioClipManager.instance.PlayAudioOneShot("SFX_EnemySpawn");
 	}

    public override StateBase UpdateEvent()
    {
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        BossProperty.waitToSpawnCount++;
        // Rigidbodyが存在する場合
        if (BossProperty.RB)
        {
            // 速度を0に固定
            BossProperty.RB.linearVelocity = Vector3.zero;
        }
        // カウントが105に達した場合
        if (BossProperty.waitToSpawnCount == 105)
        {
            BossProperty.modelObject.SetActive(true);
            // スケールを0から1へ演出
            BossProperty.modelObject.transform.localScale = Vector3.zero;
            BossProperty.modelObject.transform.DOScale(Vector3.one, 0.5f);
        }
        // カウントが140に達した場合
        if (BossProperty.waitToSpawnCount == 140)
        {
            // Shielding stateに遷移
            nextState = BossProperty.stateDictionary[BossState.Shielding];
        }
        return nextState;
    }

    public override void ExitEvent()
    {
        BossProperty.GetComponent<Collider>().enabled = true;
    }
}

// シールドステート（バリアと敵召喚）
public class BossState_ShieldingState : BossStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        BossProperty.attackDuration = 0;
        BossProperty.currentEnemyGroup =  MonoBehaviour.Instantiate( BossProperty.EnemyGroupPrefab[Random.Range(0,BossProperty.EnemyGroupPrefab.Length)],BossProperty.transform.position,Quaternion.identity).GetComponent<EnemyGroupManager>();
        BossProperty.barrierObject = MonoBehaviour.Instantiate(BossProperty.barrierPrefab,BossProperty.transform).GetComponent<BarrierObject>();
        BossProperty.barrierObject.Trigger();
        BossProperty.currentEnemyGroup.ObjsToTriggerWhenEnd.Add(BossProperty.barrierObject);
    }

    public override StateBase UpdateEvent()
    {
        // 敵グループが全滅した場合
        if (!BossProperty.currentEnemyGroup)
        {
            // WaitToMove stateに遷移
            nextState = BossProperty.stateDictionary[BossState.WaitToMove];
        }
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        // バリアが破壊された後の処理
        if (BossProperty.barrierBreakCount > 0)
        {
            if ( BossProperty.attackDuration  <  BossProperty.attackDurationMax)
            {
                BossProperty.attackDuration ++;
            }

            // 攻撃タイミングに達した場合
            if ( BossProperty.attackDuration  ==  BossProperty.attackDurationMax)
            {
                BossProperty.attackDuration  = 0;
                var tmp_effect = GameObject.Instantiate(BossProperty.chargeEndEffectPrefab,
                    BossProperty.modelObject.transform.position, Quaternion.identity);
                GameObject.Destroy(tmp_effect,2f);

                AudioClipManager.instance.PlayAudioOneShot("SFX_EnemyAttack");

                tmp_effect.transform.localScale = Vector3.one * 2.75f;
                MonoBehaviour.Destroy(
                    MonoBehaviour.Instantiate(BossProperty.AttackPrefab[Random.Range(0,BossProperty.AttackPrefab.Length)],BossProperty.modelObject.transform.position,Quaternion.Euler(0,Random.Range(0f,361f),0))
                    ,10);
            }
        }
        return nextState;
    }

    public override void ExitEvent()
    {
        BossProperty.barrierBreakCount++;
        AudioClipManager.instance.PlayAudioOneShot("SFX_BossBreak");
    }
}

// 移動待機ステート（無防備状態）
public class BossState_WaitToMoveState : BossStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        BossProperty.barrierBreakDuration = 0;
        BossProperty.barrierBreakCountMax = 240 + 10 * (1 + BossProperty.barrierBreakCount);
        MonoBehaviour.Instantiate(
            BossProperty.EnemyGroupPrefab[Random.Range(0,BossProperty.EnemyGroupPrefab.Length)],
            BossProperty.transform.position,
            Quaternion.identity);
    }
    public override StateBase UpdateEvent()
    {
        // 硬直時間が最大値に達した場合
        if (BossProperty.barrierBreakDuration >= BossProperty.barrierBreakCountMax)
        {
            // Moving stateに遷移
            nextState = BossProperty.stateDictionary[BossState.Moving];
        }
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        if (BossProperty.barrierBreakDuration < BossProperty.barrierBreakCountMax)
        {
            BossProperty.barrierBreakDuration++;
        }
        return nextState;
    }
}

// 移動ステート
public class BossState_MovingState : BossStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        int tmp_randomIndex;
        do
        {
            tmp_randomIndex = Random.Range(0,BossProperty.teleportPositions.Length);
        } while (tmp_randomIndex == BossProperty.lastPositionIndex);
        BossProperty.lastPositionIndex = tmp_randomIndex;

       BossProperty.StartCoroutine(
           MeshTrail.IEnumerator_Trail(BossProperty.trailObjectPool,BossProperty.modelObject,180,3,
               30,BossProperty.trailMaterial,"_EmissionColor"));
       BossProperty.transform.DOMove(BossProperty.teleportPositions[BossProperty.lastPositionIndex].position, 3f)
           .OnComplete(() => {
               // Shielding stateに遷移
               nextState = BossProperty.stateDictionary[BossState.Shielding];
           });
    }
    public override StateBase UpdateEvent()
    {
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        return nextState;
    }
}