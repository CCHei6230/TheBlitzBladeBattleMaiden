using UnityEngine;

public class PlayerState_JumpState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // 空中攻撃を可能に
        playerProperty.canAirAttack = true;
        // 移動速度を0、0，0に
        playerProperty.RB.linearVelocity = Vector3.zero;
        // カウントを最大値に
        playerProperty.jumpCount = playerProperty.jumpCountMax;
        // 二段ジャンプが可能な場合(二段ジャンプしていない)
        if (playerProperty.canDoubleJump)
        {
            // ジャンプアニメーションを再生
            playerProperty.animator.CrossFade(playerProperty.jumpClip.name, 0.015f );
        }
        // 二段ジャンプが不可な場合(二段ジャンプしている)
        else
        {
            // 二段ジャンプアニメーションを再生
            playerProperty.animator.Play(playerProperty.doubleJumpClip.name);
        }
    }
    public override StateBase UpdateEvent()
    {
        // 着地、かつjumpCountが０以下の場合
        if (playerProperty.isGrounding && playerProperty.jumpCount <= 0)
        {
            // ダッシュを可能に
            playerProperty.canDash = true;
            // 二段ジャンプを可能に
            playerProperty.canDoubleJump = true;
            // 空中攻撃を可能に
            playerProperty.canAirAttack = true;
            // idle stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.idle];
        }
        // 入力が可能な場合
        if (playerProperty.playerInput.canInput)
        {
            // 二段ジャンプが可能、かつジャンプボタンを押した
            // カウント < jumpCountMax - 10 (ジャンプ後10f経過)の場合
            if (playerProperty.canDoubleJump
                && playerProperty.playerInput.jumpInput.WasPerformedThisFrame()
                && playerProperty.jumpCount < playerProperty.jumpCountMax - 10)
            {
                // 空中攻撃を可能に
                playerProperty.canAirAttack = true;
                // カウントを最大値に(二段ジャンプに)
                playerProperty.jumpCount = playerProperty.jumpCountMax;
                // 二段ジャンプを不可に
                playerProperty.canDoubleJump = false;
                // 二段ジャンプエフェクトを再生
                playerProperty.jumpWaveParticles.Play();
                // SFXを再生
                AudioClipManager.instance.PlayAudioOneShot("SFX_DashOrJump");
            }
            // パリィボタンを押した、かつ空中パリィが可能な場合
            if (playerProperty.playerInput.ParryInput.WasPressedThisFrame() && playerProperty.canAirParry)
            {
                // 空中パリィを不可に
                playerProperty.canAirParry = false;
                // parry stateに遷移(空中パリィ)
                nextState = playerProperty.stateDictionary[PlayerState.parry];
            }
            // スキルボタンを押している場合
            if (playerProperty.playerInput.SkillInput.IsPressed())
            {
                // 東方向ボタンを押した、かつワープ攻撃が可能な場合
                if (  playerProperty.playerInput.SkillSlotEInput.WasPerformedThisFrame() && playerProperty.canWarpAttack)
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
                // 強攻撃ボタンを押したら
                if (playerProperty.playerInput.heavyAttackInput.WasPerformedThisFrame())
                {
                    // 空中攻撃を可能に
                    playerProperty.canAirAttack = true;
                    // heavyAttackAir stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.heavyAttackAir];
                }
                // 攻撃ボタンを押した、かつ空中攻撃が可能な場合
                if (playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame() &&  playerProperty.canAirAttack)
                {
                    // 空中攻撃を不可に
                    playerProperty.canAirAttack = false;
                    // attack stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.attack];
                }
                // ダッシュボタンを押した、かつダッシュ可能な場合
                if (playerProperty.playerInput.dashInput.WasPerformedThisFrame() && playerProperty.canDash)
                {
                    // ダッシュを不可に
                    playerProperty.canDash = false;
                    // dash stateに遷移
                    nextState=  playerProperty.stateDictionary[PlayerState.dash];
                }
            }
        }
        return nextState;
    }
    public override StateBase FixedUpdateEvent()
    {
        // 移動入力している場合
        if (playerProperty.playerInput.moveInputVector != Vector2.zero )
        {
            // ジャンプ中の移動
            PlayerStateSharedMethods.Move(playerProperty);
        }
        // 移動入力がない場合
        else
        {
            // Y軸以外の速度を0に
            playerProperty.RB.linearVelocity = new Vector3( 0, playerProperty.RB.linearVelocity.y, 0);
        }
        // ジャンプと落下
        // jumpCount > 0 の時は上昇し 、 jumpCount < 0 の時は落下する
        // 落下速度を制限
        if (playerProperty.jumpCount < -playerProperty.jumpCountMax - 5)
        {
            playerProperty.jumpCount = -playerProperty.jumpCountMax - 5;
        }
        // ジャンプの頂点に到達する前かつ、ジャンプ開始2カウント後かつボタンを放つ時、落下する
        if (playerProperty.jumpCount > 0  && 
            playerProperty.jumpCount < playerProperty.jumpCountMax -2　&&
             !playerProperty.playerInput.jumpInput.IsPressed() &&
             playerProperty.canDoubleJump
            )
        {
            playerProperty.jumpCount = 0;
        }
        // 頂点に到達時
        if (playerProperty.jumpCount == 0)
        {
            // fall stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.fall];
            return nextState;
        }
        // 上昇速度を設定
        playerProperty.RB.linearVelocity = new Vector3
            (playerProperty.RB.linearVelocity.x,
            playerProperty.jumpForce * Time.fixedDeltaTime * playerProperty.jumpCount,
            playerProperty.RB.linearVelocity.z);
      
        // カウント減算
        playerProperty.jumpCount--;
        return nextState;
    }
    public override void ExitEvent()
    {
        // カウントを最大値に
        playerProperty.jumpCount = playerProperty.jumpCountMax;
        // 移動方向を0，0，0に
        playerProperty.moveDirection = Vector3.zero;
        // Y軸以外の速度を0に
        playerProperty.RB.linearVelocity = new Vector3(0, playerProperty.RB.linearVelocity.y,0);
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
