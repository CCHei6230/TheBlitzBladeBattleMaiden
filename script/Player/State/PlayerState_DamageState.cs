using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerState_DamageState : PlayerStateBase
{

    public override void StateEnterEvent()
    {
        nextState = this;
        // ダメージ中のパリィ可能フレームを0に
        playerProperty.parryFrameWhileDamage = 0;
        playerProperty.objectCanParry = null;
        // ダメージを与えたオブジェクトの方向に向く
        playerProperty.modelObject.transform
            .DOLookAt(playerProperty.damagePosition, 0.0f, AxisConstraint.Y)
            .OnUpdate(() =>
                playerProperty.lastForward = playerProperty.modelObject.transform.forward);

        // ダメージカウントを最大値に
        playerProperty.damageCount = playerProperty.damageCountMax;
        // HPを減少させる
        playerProperty.playerHPManager.HPDecreased( playerProperty.takeDamage);
        // ダメージエフェクトを生成し、2秒後削除
        GameObject.Destroy(GameObject.Instantiate(playerProperty.damageEffectPrefab,playerProperty.modelObject.transform.position,Quaternion.identity),2f);
        // ヒットストップ
        HitStopManager.instance.HitStop(0.5f,2,true);
        // ダメージ時の集中線UIの色設定、表示する
        playerProperty.focuslineUI.SetColor(playerProperty.damagefocuslineUIColor);
        playerProperty.focuslineUI.DoFadeInOut(0.35f,0.3f);

        // HPが0以下の場合
        if (playerProperty.HP <= 0)
        {
            // death stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.death];
        }

        // 着地している場合
        if (playerProperty.isGrounding)
        {
            // ダメージアニメーションを再生
            playerProperty.animator.CrossFade(playerProperty.damageClip.name, 0 );
        }
        // 空中の場合
        else
        {
            // 空中ダメージアニメーションを再生
            playerProperty.animator.CrossFade(playerProperty.damageAirClip.name, 0 );
        }
        // ダメージボイスを再生
       AudioClipManager.instance.PlayAudioOneShot("SFX_Damage"+ Random.Range(1, 3) );
       // コントローラーの振動
       InputHaptic.instance.Haptic(0.35f,0.0f,3);
    }

    public override StateBase UpdateEvent()
    {
        // ダメージ中にパリィ可能なオブジェクトがある、かつ受付時間内の場合
        if (playerProperty.objectCanParry  != null &&   playerProperty.parryFrameWhileDamage - playerProperty.damageCount <=5)
        {
            // パリィボタンを押したら
            if (playerProperty.playerInput.ParryInput.WasPerformedThisFrame())
            {
                // オブジェクトのパリィされた処理
                playerProperty.objectCanParry .BeParry(playerProperty);
                // 空中パリィを可能に
                playerProperty.canAirParry  = true;
                // ダメージ位置に向く
                playerProperty.modelObject.transform
                    .DOLookAt(playerProperty.damagePosition, 0.0f, AxisConstraint.Y);

                // オブジェクトの生成者が存在する場合
                if (playerProperty.objectCanParry .Spawner)
                {
                    // 生成者をタグ
                    playerProperty.warpAttackManager.TagObject(playerProperty.objectCanParry .Spawner);
                }

                // parrySuccess stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.parrySuccess];
            }
        }

        // ダメージカウントが終了した場合
        if (playerProperty.damageCount <= 0)
        {
            // 着地している場合
            if (playerProperty.isGrounding)
            {
                // idle stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
            // 空中の場合
            else
            {
                // fall stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.fall];
            }
        }

        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        playerProperty.damageCount--;

        // ノックバックのY軸の速度を計算
        Vector3 tmp_YVector = Vector3.up * 450f;
        // 着地している場合はY軸の速度を0に
        if (playerProperty.isGrounding)
        {
            tmp_YVector = Vector3.zero;
        }
        playerProperty.RB.linearVelocity
            =( tmp_YVector + playerProperty.lastForward.normalized  * -playerProperty.dashSpeed * 1.25f)
        * ( (float)playerProperty.damageCount * 1.05f / (float)playerProperty.dashCountMax)
         * 0.15f* Time.fixedDeltaTime;

        return nextState;
    }

    public override void ExitEvent()
    {
        // パリィフレームを0に
        playerProperty.parryFrameWhileDamage = 0;
        // 速度を0に固定
        playerProperty.RB.linearVelocity = Vector3.zero;
        playerProperty.objectCanParry = null;
    }

    public override void TriggerEnterEvent(Collider other)
    {
        playerProperty.objectCanParry  = other.GetComponentInParent<IDamageable>();
        if (playerProperty.objectCanParry != null)
        {
            playerProperty.damagePosition = playerProperty.objectCanParry .Transform.position;
            // パリィ受付フレームを記録
            playerProperty.parryFrameWhileDamage = playerProperty.damageCount;
        }
    }

    public override void TriggerStayEvent(Collider other)
    {
        playerProperty.objectCanParry  = other.GetComponentInParent<IDamageable>();
        if (playerProperty.objectCanParry != null)
        {
            playerProperty.damagePosition = playerProperty.objectCanParry .Transform.position;
            // パリィ受付フレームを記録
            playerProperty.parryFrameWhileDamage = playerProperty.damageCount;
        }
    }
}