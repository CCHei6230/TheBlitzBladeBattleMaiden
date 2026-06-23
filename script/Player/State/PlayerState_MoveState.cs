using UnityEngine;

public class PlayerState_MoveState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // 移動アニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.moveClip.name, 0.2f );
        // 着地している場合
        if (playerProperty.isGrounding)
        {
            // 空中パリィを可能に
            playerProperty.canAirParry  = true;
        }
    }

    public override StateBase UpdateEvent()
    {
        // 移動SFXを再生
        AudioClipManager.instance.PlayAudioOneShotWhileNotPlaying("SFX_Running");

        // 着地していない場合
        if (!playerProperty.isGrounding)
        {
            // fall Stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.fall];
        }
        // 着地している場合
        else
        {
            if (playerProperty.playerInput.canInput)
            {
                // パリィボタンを押したら、parry stateに遷移
                if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
                {
                    nextState = playerProperty.stateDictionary[PlayerState.parry];
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
                    // ダッシュボタンを押した場合
                    if (playerProperty.playerInput.dashInput.WasPerformedThisFrame())
                    {
                        // dash stateに遷移
                        nextState=  playerProperty.stateDictionary[PlayerState.dash];
                    }
                    // ジャンプボタンを押したら
                    if (playerProperty.playerInput.jumpInput.WasPerformedThisFrame())
                    {

                        // jump stateに遷移
                        nextState =  playerProperty.stateDictionary[PlayerState.jump];
                    }
                }
            }
        }
        return nextState;
    }
    public override StateBase FixedUpdateEvent()
    {
        // 入力が可能な場合
        if (playerProperty.playerInput.canInput)
        {
            // 移動入力している場合
            if (playerProperty.playerInput.moveInputVector != Vector2.zero)
            {
                // 移動処理
                PlayerStateSharedMethods.Move(playerProperty);
            }
            // 移動入力がない場合
            else
            {
                // idle Stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
        }
        // 入力できない場合
        else
        {
            // idle Stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.idle];
        }
        return nextState;
    }
    public override void ExitEvent()
    {
        // 移動SFXを停止
        AudioClipManager.instance.StopAudio("SFX_Running");
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
