using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class PlayerState_DashState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState  = this;
        // モデルのDO系処理を中止
        playerProperty.modelObject.transform.DOComplete();
        // パリィ可能オブジェクトをnullに
        playerProperty.objectCanParry = null;
        // ダッシュカウントを最大値に
        playerProperty.dashCount = playerProperty.dashCountMax;
        // 入力を不可に
        playerProperty.playerInput.canInput  = false;

        // 移動入力している場合
        if (playerProperty.playerInput.moveInputVector != Vector2.zero)
        {
            // モデルを移動方向に向ける
            PlayerStateSharedMethods.RotateModel(playerProperty, 0.0f);
        }
        // ロックオンしている場合
        else if (playerProperty.lockOnEnemy)
        {
            // モデルをターゲットに向ける
            PlayerStateSharedMethods.  RotateModelToLockOnedEnemy(playerProperty, 0.0f);
        }
        // 着地している場合
        if (playerProperty.isGrounding)
        {
            // ダッシュアニメーションを再生
            playerProperty.animator.CrossFade(playerProperty.dashClip.name, 0.02f );
        }
        // 空中ダッシュの場合
        else
        {
            // 空中ダッシュアニメーションを再生
            playerProperty.animator.CrossFade(playerProperty.dashAirClip.name, 0.02f );
        }
        // ダッシュエフェクトを再生
        playerProperty.dashWaveParticles.Play();
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_DashOrJump");
    }
    public override StateBase UpdateEvent()
    {
        // パリィ可能なオブジェクトがある場合
        if (playerProperty.objectCanParry  != null)
        {
            // パリィボタンを押したら
            if (playerProperty.playerInput.ParryInput.WasPerformedThisFrame())
            {
                // オブジェクトのパリィされた処理
                playerProperty.objectCanParry .BeParry(playerProperty);
                // 空中パリィを可能に
                playerProperty.canAirParry  = true;
                // モデルをダメージ位置に向ける
                playerProperty.modelObject.transform
                    .DOLookAt(playerProperty.damagePosition, 0.0f, AxisConstraint.Y);
                // オブジェクトは生成者が存在する場合(敵から放った攻撃)
                if (playerProperty.objectCanParry .Spawner)
                {
                    // 生成者をタグ
                    playerProperty.warpAttackManager.TagObject(playerProperty.objectCanParry .Spawner);
                }
                // parrySuccess stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.parrySuccess];
            }
        }
        // ダッシュカウントが最大値の半分以下の場合
        if (playerProperty.dashCount <= playerProperty.dashCountMax / 2)
        {
            // パリィボタンを押したら
            if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
            {
                // 着地している場合
                if (playerProperty.isGrounding)
                {
                    // parry stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.parry];
                }
                // 空中の場合
                else
                {
                    // 空中パリィが可能な場合
                    if(playerProperty.canAirParry)
                    {
                        // 空中パリィを不可に
                        playerProperty.canAirParry = false;
                        // parry stateに遷移
                        nextState = playerProperty.stateDictionary[PlayerState.parry];
                    }
                }
            }
        }
        // ダッシュが終了した場合
        if (playerProperty.dashCount <= 0)
        {
            // 着地していない場合
            if (!playerProperty.isGrounding)
            {
                // 空中攻撃を可能に
                playerProperty.canAirAttack = true;    
                //fall stateに遷移
                nextState  =  playerProperty.stateDictionary[PlayerState.fall];
            }
            // 着地している場合
            else
            {
                // 移動入力している場合
                if (playerProperty.playerInput.moveInputVector != Vector2.zero)
                {
                    // move stateに遷移
                    nextState  =  playerProperty.stateDictionary[PlayerState.move];
                }
                // 移動入力がない場合
                else
                {
                    // idle stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.idle];
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
            // 攻撃ボタンを押したら
            if (playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame())
            {
                // 着地していない場合
                if (!playerProperty.isGrounding)
                {
                    // 空中攻撃を可能に
                    playerProperty.canAirAttack = true;
                }
                // dashAttack stateに遷移
                nextState  =  playerProperty.stateDictionary[PlayerState.dashAttack];
            }
            // 強攻撃ボタンを押したら
            if (playerProperty.playerInput.heavyAttackInput.WasPerformedThisFrame())
            {
                // 着地している場合
                if (playerProperty.isGrounding)
                {
                    // heavyAttack stateに遷移
                    nextState  =  playerProperty.stateDictionary[PlayerState.heavyAttack];
                }
                // 空中の場合
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
        playerProperty.dashCount--;
        // ダッシュ速度を適用
        playerProperty.RB.linearVelocity 
            = playerProperty.lastForward.normalized *  
              ( (float)playerProperty.dashCount * 1.05f / (float)playerProperty.dashCountMax)
              * playerProperty.dashSpeed*  Time.fixedDeltaTime;
        return nextState;
    }
    public override void ExitEvent()
    {
        // パリィ可能オブジェクトをnullに
        playerProperty.objectCanParry = null;
        // 速度を0に固定
        playerProperty.RB.linearVelocity = Vector3.zero;
        // 入力を可能に
        playerProperty.playerInput.canInput  = true;
        // ダッシュカウントを0に
        playerProperty.dashCount = 0;
    }

    public override void TriggerEnterEvent(Collider other)
    {
        // パリィ可能なオブジェクトを取得
        playerProperty.objectCanParry  = other.GetComponent<IDamageable>();
        if (playerProperty.objectCanParry != null) 
        {   
            // ダメージ位置を更新
            playerProperty.damagePosition = playerProperty.objectCanParry .Transform.position;
        }
        // ダッシュ時間が残り1/3以下の時、無敵効果を解除
        if (playerProperty.dashCount <= playerProperty.dashCountMax / 3f)
        {
            // ダメージ処理
            nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
        }
    }

    public override void TriggerStayEvent(Collider other)
    {
        // パリィ可能なオブジェクトを取得
        playerProperty.objectCanParry  = other.GetComponent<IDamageable>();
        if (playerProperty.objectCanParry != null) 
        {   
            // ダメージ位置を更新
            playerProperty.damagePosition = playerProperty.objectCanParry .Transform.position;
        }
        // ダッシュ時間が残り1/3以下の時、無敵効果を解除
        if (playerProperty.dashCount <= playerProperty.dashCountMax / 3f)
        {
            // ダメージ処理
            nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
        }
    }
}
