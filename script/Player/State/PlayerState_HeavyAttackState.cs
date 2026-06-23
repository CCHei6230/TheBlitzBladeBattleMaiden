using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class PlayerState_HeavyAttackState: PlayerStateBase
{
    public override void StateEnterEvent()
    {
        Debug.Log("AttackStart");
        nextState = this;
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_HAtk");
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
        // 地上強攻撃を取得
        playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerHeavyAttack;
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
                PlayerStateSharedMethods.RotateModelToLockOnedEnemy(playerProperty, 0.1f);
            }
            else
            {
                // 付近の敵をサーチしてその敵に向く
                playerProperty.modelObject.transform.DOComplete();
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
                            new Vector2(enemyCol.ModleTransform.position.x, enemyCol.ModleTransform.position.z),
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
                            .DOLookAt(tmp_nearestEnemycol.ModleTransform.position, 0.1f, AxisConstraint.Y)
                            .OnUpdate(() =>
                                playerProperty.lastForward = playerProperty.modelObject.transform.forward);
                    }
                }
            }
        }
        // 強攻撃アニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.atkSObjectCurrent.animClip.name, 0.05f );
    }

    public override StateBase UpdateEvent()
    {
        // 攻撃終了後の遷移
        if (playerProperty.atkCount >= playerProperty.atkCountMax)
        {
            if (playerProperty.isGrounding)
            {
                // idle stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
            else
            {
                // fall stateに遷移
               nextState = playerProperty.stateDictionary[PlayerState.fall];
            }
        }

        // 移動キャンセル可能フレームを過ぎた場合
        if (playerProperty.atkCount >= playerProperty.atkSObjectCurrent.frameCanMove)
        {
            // 移動入力がある場合
            if (playerProperty.playerInput.moveInputVector!=Vector2.zero)
            {
                // move stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.move];
            }
        }


        // 遷移の入力受付期間
        if (playerProperty.atkCount <playerProperty.atkSObjectCurrent.frameDenyPerformNextMotion &&playerProperty.atkCount >playerProperty.atkSObjectCurrent.frameCanPerformNextMotion )
        {
            // 通常攻撃ボタンを押した場合
            if (playerProperty.playerInput.normalAttackInput.WasPressedThisFrame() )
            {
                // attack stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.attack];
            }
            // 強攻撃ボタンを押した場合
            if (playerProperty.playerInput.heavyAttackInput.WasPressedThisFrame())
            {
                // heavyAttackAir stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.heavyAttackAir];
            }
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
        else
        {
            // ダッシュボタンを押した、かつダッシュ可能な場合
            if (playerProperty.playerInput.dashInput.WasPressedThisFrame() && playerProperty.canDash )
            {
                // ダッシュ可能をfalseに
                playerProperty.canDash = false;
                // dash stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.dash];
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
