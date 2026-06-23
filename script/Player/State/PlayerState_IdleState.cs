using DG.Tweening;
using UnityEngine;

public class PlayerState_IdleState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // 空中攻撃を可能に
        playerProperty.canAirAttack = true;
        // 空中パリィを可能に
        playerProperty.canAirParry  = true;
        // Idleアニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.idleClip.name, 0.2f );
    }
    public override StateBase UpdateEvent()
    {
        // 速度を0に固定
        playerProperty.RB.linearVelocity =Vector3.zero;
        // 着地中
        if (playerProperty.isGrounding)
        {
            // パリィボタンを押したら
            if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
            {
                // parry stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.parry];
            }
            // 移動入力している場合
            if (playerProperty.playerInput.moveInputVector!=Vector2.zero)
            {
                // move stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.move];
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
                    // 南方向ボタンを押した場合
                    if ( playerProperty.playerInput.SkillSlotSInput.WasPerformedThisFrame())
                    {
                        // skill3 stateに遷移 (落花ノ構え)
                        nextState = playerProperty.stateDictionary[PlayerState.skill3];
                    }
                }
            }
            else
            {
                // ジャンプボタンを押したら
                if (playerProperty.playerInput.jumpInput.WasPerformedThisFrame())
                {
                    // 二段ジャンプを可能に
                    playerProperty.canDoubleJump = true;
                    // jump stateに遷移
                    nextState =  playerProperty.stateDictionary[PlayerState.jump];
                }
                // 攻撃ボタンを押したら
                if (playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame())
                {
                    // attack stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.attack];
                }
                // 強攻撃ボタンを押したら
                if (playerProperty.playerInput.heavyAttackInput.WasPerformedThisFrame())
                {
                    // heavyAttack stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.heavyAttack];
                }
                // ダッシュボタンを押した、かつダッシュ可能な場合
                if (playerProperty.playerInput.dashInput.WasPerformedThisFrame() && playerProperty.canDash)
                {
                    // dash stateに遷移
                    nextState=  playerProperty.stateDictionary[PlayerState.dash];
                }
            }
        }
        else
        {
            // fall stateに遷移
            nextState =  playerProperty.stateDictionary[PlayerState.fall];
        }
        return nextState;
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
