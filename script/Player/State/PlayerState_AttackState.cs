using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;

public class PlayerState_AttackState : PlayerStateBase
{
    // 攻撃データを設定する処理
    void SetAttackData()
    {
        //攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
        // 着地している場合
        if (playerProperty.isGrounding)
        {
            //地上攻撃を取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerNComboList[playerProperty.atkCombo];
        }
        // 着地していない場合
        else
        {
            //空中攻撃を取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerAirNComboList[playerProperty.atkCombo];
        }
        //攻撃の基本データを設定
        PlayerStateSharedMethods.SetAttackData(playerProperty);
        // 移動入力している場合
        if (playerProperty.playerInput.moveInputVector != Vector2.zero)
        {
            //モデルを移動方向に向ける
            PlayerStateSharedMethods.RotateModel(playerProperty, 0.2f);
        }
        else
        {
            // ロックオンしている場合
            if (playerProperty.lockOnEnemy)
            {
                //モデルをターゲットに向ける
                PlayerStateSharedMethods.RotateModelToLockOnedEnemy(playerProperty, 0.1f);
                
                //距離を計算
                float tmp_distance = 0;
                tmp_distance = Vector2.Distance(
                    new Vector2(playerProperty.lockOnEnemy.position.x, playerProperty.lockOnEnemy.position.z),
                    new Vector2(playerProperty.modelObject.transform.position.x,
                        playerProperty.modelObject.transform.transform.position.z));
                tmp_distance = math.clamp(tmp_distance, 0, 20);
                // 距離に応じて前進速度を調整
                if (tmp_distance < 5f)
                {
                    playerProperty.atkForwardSpeed *= 0.5f *  tmp_distance/10f;
                }
                else
                {
                    playerProperty.atkForwardSpeed  *= 1.1f * tmp_distance/10f;
                }
            }
            // ロックオンしていない場合、付近の敵をサーチ
            else
            {
                // モデルのDO系処理を中止
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
                // 敵が見つかった場合
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
                    // 最も近い敵の方向に向く
                    if (tmp_nearestEnemycol != null)
                    {
                        playerProperty.modelObject.transform
                            .DOLookAt(tmp_nearestEnemycol.ModleTransform.position, 0.1f, AxisConstraint.Y)
                            .OnUpdate(() =>
                                playerProperty.lastForward = playerProperty.modelObject.transform.forward);
                    }
                    // 至近距離の場合、前進速度を制限
                    if (nearestDistance < 2f)
                    {
                        playerProperty.atkForwardSpeed  *= 0.5f *  nearestDistance/10f;
                    }

                }
            }
        }
        //攻撃アニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.atkSObjectCurrent.animClip.name, 0.08f );

        // 最終コンボの場合
        if ((playerProperty.atkCombo == playerProperty.playerComboList.playerAirNComboList.Length - 1))
        {
            //フィニッシュ音を再生
            AudioClipManager.instance.PlayAudioOneShot("SFX_AttackFinish");
        }
        else
        {
            //通常攻撃音を再生
            AudioClipManager.instance.PlayAudioOneShot("SFX_Attack");
        }

    }

    public override void StateEnterEvent()
    {
        nextState = this;
        //攻撃データを設定
        SetAttackData();
    }

    public override StateBase UpdateEvent()
    {
        // パリィボタンを押したら
        if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
        {
            // parry stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.parry];
        }
        // 攻撃カウントが最大値以上の場合
        if (playerProperty.atkCount >= playerProperty.atkCountMax)
        {
            // 着地している場合
            if(playerProperty.isGrounding)
            {
                // idle stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
            // 着地していない場合
            else
            {
                // fall stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.fall];
            }
        }

        //移動入力によるキャンセル
        if (playerProperty.playerInput.moveInputVector!=Vector2.zero &&
            playerProperty.atkCount >playerProperty.atkSObjectCurrent.frameCanMove)
        {
            // move stateに遷移
            nextState =  playerProperty.stateDictionary[PlayerState.move];
        }

        // コンボ遷移の受付期間
        if (playerProperty.atkCount >= playerProperty.atkSObjectCurrent.frameCanPerformNextMotion && playerProperty.atkCount<playerProperty.atkSObjectCurrent.frameDenyPerformNextMotion  )
        {
            // 攻撃ボタンまたは先行入力がある場合
            if (playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame()||playerProperty.atkPreInput )
            {
                if (playerProperty.atkPreInput)
                {
                    //先行入力フラグをfalseに
                    playerProperty.atkPreInput = false;
                }

                // 着地している場合
                if (playerProperty.isGrounding)
                {
                    // コンボが最大値より小さくない場合
                    if ((playerProperty.atkCombo < playerProperty.playerComboList.playerNComboList.Length - 1))
                    {
                        playerProperty.atkCombo++;
                    }
                    else
                    {
                        //コンボを0に(コンボ終了)
                        playerProperty.atkCombo = 0;
                    }
                }
                // 空中の場合
                else
                {
                    // コンボが最大値より小さくない場合
                    if ((playerProperty.atkCombo < playerProperty.playerComboList.playerAirNComboList.Length - 1))
                    {
                        playerProperty.atkCombo++;
                    }
                    else
                    {
                        //コンボを0に(コンボ終了)
                        playerProperty.atkCombo = 0;
                    }
                }
                //次の攻撃データを設定
                SetAttackData();
            }
        }
        // 先行入力の受付
        else  if (playerProperty.atkCount >1  )
        {
            // 攻撃ボタンを押した、かつコンボが最大値より小さくない場合
            if (playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame()
                && (playerProperty.atkCombo < playerProperty.playerComboList.playerNComboList.Length-1)
                )
            {
                //先行入力をtrueに
                if (!playerProperty.atkPreInput)
                {
                    playerProperty.atkPreInput = true;
                }
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
                // 南方向ボタンを押した、かつ着地中の場合
                if ( playerProperty.playerInput.SkillSlotSInput.WasPerformedThisFrame() && playerProperty.isGrounding)
                {
                    // skill3 stateに遷移 (落花ノ構え)
                    nextState = playerProperty.stateDictionary[PlayerState.skill3];
                }
            }
        }
        else
        {
            // ダッシュによるキャンセル
            if (playerProperty.playerInput.dashInput.WasPerformedThisFrame() && playerProperty.canDash )
            {
                //ダッシュ可能をfalseに
                playerProperty.canDash = false;
                // dash stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.dash];
            }
            // 強攻撃によるキャンセル
            if (playerProperty.playerInput.heavyAttackInput.WasPerformedThisFrame())
            {
                // 着地している場合
                if (playerProperty.isGrounding)
                {
                    // heavyAttack stateに遷移
                    nextState =  playerProperty.stateDictionary[PlayerState.heavyAttack];
                }
                // 着地していない場合
                else
                {
                    // heavyAttackAir stateに遷移
                    nextState =  playerProperty.stateDictionary[PlayerState.heavyAttackAir];
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
        playerProperty.atkPreInput  = false;
        playerProperty.atkCount = 0;
        playerProperty.atkCombo = 0;
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
