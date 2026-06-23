using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
public class PlayerState_HeavyAttackAirState: PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        //SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_HAtkAir");
    
        // 空中強攻撃を取得
        playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerHeavyAirAttack;
        // 攻撃の基本データを設定
        PlayerStateSharedMethods.SetAttackData(playerProperty);
        // 移動入力している場合
        if (playerProperty.playerInput.moveInputVector != Vector2.zero)
        {
            // モデルを移動方向に向ける
            PlayerStateSharedMethods.RotateModel(playerProperty, 0.2f);
        }
        else
        {
            // ロックオンしている場合
            if (playerProperty.lockOnEnemy)
            {
                // モデルをターゲットに向ける
                PlayerStateSharedMethods.  RotateModelToLockOnedEnemy(playerProperty, 0.1f);
            }
            else
            {
                // モデルのDO系処理を中止
                playerProperty.modelObject.transform.DOComplete();

                // 付近の敵をサーチしてその敵に向く
                Collider[] tmp_col = Physics.OverlapSphere(playerProperty.modelObject.transform.position, 25);
               List<IAttackable> tmp_hittedEnemy  = new List<IAttackable>();
               foreach (var col in tmp_col)
               {
                   col.TryGetComponent(out IAttackable tmp_attackable);
                   if (tmp_attackable !=null)
                   {
                       tmp_hittedEnemy.Add(tmp_attackable);
                   }
               }
                if (tmp_hittedEnemy.Count != 0)
                {
                    float nearestDistance = 25f;
                    float tmp_distance = 0;
                    IAttackable tmp_nearestEnemycol = null;
                    foreach (var enemyCol in tmp_hittedEnemy)
                    {
                        tmp_distance = Vector2.Distance(
                            new Vector2(enemyCol.ModelTransform.position.x, enemyCol.ModelTransform.position.z),
                            new Vector2(playerProperty.modelObject.transform.position.x,
                                playerProperty.modelObject.transform.transform.position.z));
                        if (tmp_distance < nearestDistance)
                        {
                            nearestDistance = tmp_distance;
                            tmp_nearestEnemycol = enemyCol;
                        }
                    }

                    if (tmp_nearestEnemycol != null)
                    {
                        playerProperty.modelObject.transform
                            .DOLookAt(tmp_nearestEnemycol.ModelTransform.position, 0.1f, AxisConstraint.Y)
                            .OnUpdate(() =>
                                playerProperty.lastForward = playerProperty.modelObject.transform.forward);
                    }
                }
            }
        }
        // 空中強攻撃アニメーションを再生
        playerProperty.animator.CrossFade( playerProperty.atkSObjectCurrent.animClip.name, 0f );
    }

    public override StateBase UpdateEvent()
    {

        // 着地、かつ落下開始フレームを過ぎた場合
        if(playerProperty.isGrounding && playerProperty.atkCount >  playerProperty.atkSObjectCurrent.frameStartMoveDown)
        {
            // heavyAttackGround stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.heavyAttackGround];
        }

        // スキルボタンを押している場合
        if (playerProperty.playerInput.SkillInput.IsPressed())
        {
            // 東方向ボタンを押した、かつワープ攻撃が可能な場合
            if (playerProperty.playerInput.SkillSlotEInput.WasPerformedThisFrame() && playerProperty.canWarpAttack)
            {
                // warpAttack stateに遷移 (紫電一閃)
                nextState = playerProperty.stateDictionary[PlayerState.warpAttack];
            }

            // 攻撃レベルが1より大きい場合
            // (攻撃レベルを消費してスキルを使う)
            if (playerProperty.playerAttackLv > 1)
            {
                // 北方向ボタンを押した場合
                if ( playerProperty.playerInput.SkillSlotNInput.WasPerformedThisFrame())
                {
                    // skill1 stateに遷移 (千雷刃)
                    nextState = playerProperty.stateDictionary[PlayerState.skill1];
                }
                // 西方向ボタンを押した場合
                if ( playerProperty.playerInput.SkillSlotWInput.WasPerformedThisFrame())
                {
                    // skill2 stateに遷移 (桜花疾走)
                    nextState = playerProperty.stateDictionary[PlayerState.skill2];
                }
            }
        }


        return nextState;
    }


    public override StateBase FixedUpdateEvent()
    {
        // 攻撃のFixedUpdate処理
        PlayerStateSharedMethods.AttackFixedUpdate(playerProperty);
        return this;
    }
    public override void ExitEvent()
    {
        // 攻撃カウントを0に
        playerProperty.atkCount = 0;
        // 最大値を0に
        playerProperty.atkCountMax = 0;
        // コンボを0に
        playerProperty.atkCombo = 0;
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
    }

    // ダメージ判定
    public override void TriggerEnterEvent(Collider other)
    {
        nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
    }
    public override void TriggerStayEvent(Collider other)
    {
        nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
    }
}
