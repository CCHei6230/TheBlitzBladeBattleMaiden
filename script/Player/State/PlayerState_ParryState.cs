using DG.Tweening;
using UnityEngine;

public class PlayerState_ParryState  : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // パリィカウントを0に
        playerProperty.parryCount = 0;
        // 最大値を55に
        playerProperty.parryCountMax = 55;
        // パリィ可能の最大値を20に
        playerProperty.parryCanCountMax = 20;

        // 着地している場合
        if (playerProperty.isGrounding)
        {
            // パリィアニメーションを再生
            playerProperty.animator.Play(playerProperty.parryClip.name);
        }
        // 空中の場合
        else
        {
            // 空中パリィを不可に
            playerProperty.canAirParry = false;
            // 空中パリィアニメーションを再生
            playerProperty.animator.Play(playerProperty.parryAirClip.name);
        }
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_ParryVoice");
    }

    public override StateBase UpdateEvent()
    {
        // スキルボタンと攻撃ボタンを同時に押した場合
        if (playerProperty.playerInput.SkillInput.IsPressed() && playerProperty.playerInput.normalAttackInput.WasPerformedThisFrame() && playerProperty.canWarpAttack)
        {
            // warpAttack stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.warpAttack];
        }
        else
        {
            // パリィボタンを再度押した、カウントが5以上
            if (playerProperty.playerInput.ParryInput.WasPressedThisFrame() && playerProperty.parryCount >5)
            {
                // 着地中
                if(playerProperty.isGrounding)
                {
                    // カウントを0に
                    playerProperty.parryCount = 0;
                    // アニメーションを再生
                    playerProperty.animator.Play(playerProperty.parryClip.name,0,0);
                    // SFXを再生
                    AudioClipManager.instance.PlayAudioOneShot("SFX_ParryVoice");
                }
                // 空中
                else
                {
                    // 空中パリィが可能な場合
                    if (playerProperty.canAirParry)
                    {
                        // 空中パリィをfalseに
                        playerProperty.canAirParry = false;
                        // カウントを0に
                        playerProperty.parryCount = 0;
                        // アニメーションを再生
                        playerProperty.animator.Play(playerProperty.parryAirClip.name,0,0);
                        // SFXを再生
                        AudioClipManager.instance.PlayAudioOneShot("SFX_ParryVoice");
                    }
                }
            }

            // ダッシュボタンを押した場合
            if (playerProperty.playerInput.dashInput.WasPressedThisFrame())
            {
                // 空中かつダッシュ可能な場合
                if (!playerProperty.isGrounding && playerProperty.canDash)
                {
                    // 空中パリィをtrueに
                    playerProperty.canAirParry  = true;
                    // ダッシュ可能をfalseに
                    playerProperty.canDash = false;
                    // dash stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.dash];
                }
                else if (playerProperty.isGrounding)
                {
                    // 空中パリィをtrueに
                    playerProperty.canAirParry  = true;
                    // dash stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.dash];
                }
            }
        }

        // カウント50以上の場合
        if (playerProperty.parryCount > 50)
        {
            // 移動入力がある場合
            if (playerProperty.playerInput.moveInputVector!= Vector2.zero)
            {
                // 着地判定で空中パリィを設定
                playerProperty.canAirParry  = playerProperty.isGrounding;
                // move stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.move];
            }
        }
        // カウントが最大値以上の場合
        if (playerProperty.parryCount >= playerProperty.parryCountMax)
        {
            // 着地中
            if (playerProperty.isGrounding)
            {
                // 空中パリィをtrueに
                playerProperty.canAirParry  = true;
                // idle stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
            // 空中
            else
            {
                // 空中パリィをfalseに
                playerProperty.canAirParry  = false;
                // fall stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.fall];
            }
        }
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        // カウント加算
        if (playerProperty.parryCount < playerProperty.parryCountMax)
        {
            playerProperty.parryCount++;
        }
        // 速度を減速
        playerProperty.RB.linearVelocity = Vector3.Lerp( Vector3.zero,playerProperty.RB.linearVelocity,Time.fixedDeltaTime);
        return this;
    }

    public override void ExitEvent(){}

    // ダメージ判定（パリィ成功判定）
    public override void TriggerEnterEvent(Collider other)
    {
        // パリィ受付期間内の場合
        if (playerProperty.parryCount < playerProperty.parryCanCountMax)
        {
            var  tmp_damage = other.GetComponentInParent<iDamageable>();
            if (tmp_damage != null)
            {
                // 敵をパリィ
                tmp_damage.BeParry(playerProperty);
                // 空中パリィを可能に
                playerProperty.canAirParry  = true;
                // ダメージ位置を保存
                playerProperty.damagePosition = tmp_damage.Transform.position;
                // 敵の方向に向く
                playerProperty.modelObject.transform
                    .DOLookAt(playerProperty.damagePosition, 0.0f, AxisConstraint.Y);
                // parrySuccess stateに遷移
                nextState= playerProperty.stateDictionary[PlayerState.parrySuccess];

                // Spawnerが存在する場合、ターゲットをタグ
                if (tmp_damage.Spawner)
                {
                    playerProperty.warpAttackManager.TagObject(tmp_damage.Spawner);
                }
            }
        }
        // 受付期間外の場合、ダメージ
        else
        {
            nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
        }
    }
}