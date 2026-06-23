using UnityEngine;

public class PlayerState_HeavyAttackAirGroundState: PlayerStateBase
{
    public override void StateEnterEvent()
    {
        // 着地硬直のカウントを最大値に
        playerProperty.heavyAttackAirGroundCount = playerProperty.heavyAttackAirGroundCountMax;
        // 着地アニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.heavyAttackAirGroundClip.name, 0 );
        // 速度を0に固定
        playerProperty.RB.linearVelocity = Vector3.zero;
        // 二段ジャンプを可能に
        playerProperty.canDoubleJump = true;
        // 強攻撃の着地エフェクトを再生
        playerProperty.heavyAttackWaveParticles.Play();
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_HAtkAirGrounded");
        // コントローラーの振動を入れる
        InputHaptic.instance.Haptic(0.5f,0,2);
        nextState = this;
    }

    public override StateBase UpdateEvent()
    {
        // パリィボタンを押したら
        if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
        {
            // parry stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.parry];
        }
        // 硬直カウントが20以下の場合
        if (playerProperty.heavyAttackAirGroundCount < 20)
        {
            // 移動入力している場合
            if (playerProperty.playerInput.moveInputVector!=Vector2.zero  )
            {
                // move stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.move];
            }
            // 強攻撃ボタンを押した場合
            if (playerProperty.playerInput.heavyAttackInput.WasPerformedThisFrame())
            {
                // heavyAttack stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.heavyAttack];
            }
            // 通常攻撃ボタンを押した場合
            if (playerProperty.playerInput.normalAttackInput.WasPressedThisFrame() )
            {
                // attack stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.attack];
            }
            // ジャンプボタンを押した場合
            if (playerProperty.playerInput.jumpInput.WasPressedThisFrame() )
            {
                // jump stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.jump];
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
            // ダッシュボタンを押した場合
            if (playerProperty.playerInput.dashInput.WasPerformedThisFrame() && playerProperty.canDash )
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
        playerProperty.heavyAttackAirGroundCount--;
        // カウントが0以下の場合
        if (playerProperty.heavyAttackAirGroundCount <= 0)
        {
            // idle stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.idle];
        }
        return nextState;
    }
    public override void ExitEvent()
    {
        // カウントを0に
        playerProperty.heavyAttackAirGroundCount = 0;
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
