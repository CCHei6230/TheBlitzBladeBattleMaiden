using UnityEngine;
public class PlayerState_DashAttackState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // 着地している場合
        if (playerProperty.isGrounding)
        {
            // 地上ダッシュ攻撃の設定を取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerDashAttack;
        }
        // 空中の場合
        else
        {
            // 空中ダッシュ攻撃の設定を取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerDashAirAttack;
        }
        // 先行入力をfalseに
        playerProperty.atkPreInput = false;
        // エフェクトインデックスを初期化
        playerProperty.atkEffectIndex = 0;
        // 攻撃の基本データを設定
        PlayerStateSharedMethods.SetAttackData(playerProperty);
        // ダッシュ攻撃アニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.atkSObjectCurrent.animClip.name, 0.05f );
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Attack");
    }

    public override StateBase UpdateEvent()
    {
        // 攻撃終了後の遷移
        if (playerProperty.atkCount >= playerProperty.atkCountMax)
        {
            // 着地中の場合
            if(playerProperty.isGrounding)
            {
                // idle stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
            // 着地してない場合
            else
            {
                // fall stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.fall];
            }

        }
        // 移動入力によるキャンセル
        if (playerProperty.playerInput.moveInputVector!=Vector2.zero && playerProperty.atkCount >playerProperty.atkSObjectCurrent.frameCanMove  )
        {
            // 着地中の場合
            if(playerProperty.isGrounding)
            {
                // move stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.move];
            }
            // 着地してない場合
            else
            {
                // fall stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.fall];
            }
        }
        // コンボ遷移の受付期間
        if (playerProperty.atkCount >= playerProperty.atkSObjectCurrent.frameCanPerformNextMotion   )
        {
            // 攻撃ボタンまたは先行入力がある場合
            if ((playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame()||playerProperty.atkPreInput)&& (playerProperty.canAirAttack||playerProperty.isGrounding))
            {
                if (playerProperty.atkPreInput)
                {
                    // 先行入力をfalseに
                    playerProperty.atkPreInput = false;
                }
                // 通常攻撃コンボの2段目に遷移
                playerProperty.atkCombo = 1;
                // 空中攻撃をfalseに
                playerProperty.canAirAttack   = false;
                // attack stateに遷移
                nextState =  playerProperty.stateDictionary[PlayerState.attack];
            }
            // ダッシュによるキャンセル
            if (playerProperty.playerInput.dashInput.WasPerformedThisFrame() && playerProperty.canDash )
            {
                // ダッシュ可能をfalseに
                playerProperty.canDash = false;
                // dash stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.dash];
            }
        }
        else
        {
            // 先行入力の受付
            if (playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame() && !playerProperty.atkPreInput)
            {
                playerProperty.atkPreInput   = true;
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
            // 強攻撃によるキャンセル
            if (playerProperty.playerInput.heavyAttackInput.WasPerformedThisFrame())
            {
                // 着地中の場合
                if (playerProperty.isGrounding)
                {
                    // heavyAttack stateに遷移
                    nextState  =  playerProperty.stateDictionary[PlayerState.heavyAttack];
                }
                // 着地してない場合
                else
                {
                    // heavyAttackAir stateに遷移
                    nextState  =  playerProperty.stateDictionary[PlayerState.heavyAttackAir];
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
    // public override StateBase LateUpdateEvent(){return this; }
    public override void ExitEvent()
    {
        // カウントを0に
        playerProperty.atkCount = 0;
        // カウント最大値を0に
        playerProperty.atkCountMax = 0;
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
    }

    // ダメージ処理
    public override void TriggerEnterEvent(Collider other)
    {
        nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
    }
    public override void TriggerStayEvent(Collider other)
    {
        nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
    }
}
