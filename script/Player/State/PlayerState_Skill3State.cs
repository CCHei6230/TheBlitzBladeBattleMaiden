using UnityEngine;
using DG.Tweening;

public class PlayerState_Skill3State : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // 攻撃レベルを下げる
        playerProperty.PlayerAttackLvDecreased();
        // スキル3のカウントを0に
        playerProperty.skill3Count =0;
        // パリィ可能な最大フレームを設定
        playerProperty.parryCanCountMax =80;
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
        // 現攻撃のPlayerAttackSObjをスキル3に設定
        playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerSkillList[2];
        // エフェクトインデックスを初期化
        playerProperty.atkEffectIndex = 0;
        // パリィ成功後のカウントを初期化
        playerProperty.skill3AfterParrySuccessCount = -1;
        // パリィカウント最大値をスキル3のdurationに設定
        playerProperty.skill3AtkCountMax = playerProperty.atkSObjectCurrent.duration;
        // 速度を0に固定
        playerProperty.RB.linearVelocity = Vector3.zero;
        // コントローラーを振動させる
        InputHaptic.instance.Haptic(0.25f,0,2);
        // スキル3アニメーションを再生
        playerProperty.animator.Play(playerProperty.skill3Clip.name);
        // スキル3のエフェクトを生成
        MonoBehaviour.Instantiate(playerProperty.skill3EffectPrefab, playerProperty.modelObject.transform.position, Quaternion.identity);
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Skill3");
    }

    public override StateBase UpdateEvent()
    {
        // パリィ成功後のカウントが最大値以上の場合(パリィ成功からの追加攻撃が終了)
        if (playerProperty.skill3AfterParrySuccessCount >= playerProperty.skill3AtkCountMax )
        {
            // idle stateに遷移
           nextState = playerProperty.stateDictionary[PlayerState.idle];
        }
        // 移動入力している、かつパリィ成功後のカウントが移動可能フレームより大きい場合
        if (playerProperty.playerInput.moveInputVector!=Vector2.zero && playerProperty.skill3AfterParrySuccessCount >playerProperty.atkSObjectCurrent.frameCanMove)
        {
            // move stateに遷移
            nextState =  playerProperty.stateDictionary[PlayerState.move];
        }
        // スキル3のカウントが最大フレームより大きい場合(パリィ失敗)
        if (playerProperty.skill3Count > playerProperty.skill3CountMax)
        {
            // idle stateに遷移
            nextState =  playerProperty.stateDictionary[PlayerState.idle];
        }
        return nextState;
    }
    public override StateBase FixedUpdateEvent()
    {
        // パリィ成功後のカウントが攻撃開始のフレームと同じ場合
        if (playerProperty.skill3AfterParrySuccessCount == playerProperty.atkSObjectCurrent.frameStartCollider)
        {
            // 当たり判定の位置を設定
            playerProperty.attackCollider.transform.localPosition = playerProperty.atkSObjectCurrent.colliderPosition;
            // 当たり判定のサイズを設定
            playerProperty.attackCollider.transform.localScale = playerProperty.atkSObjectCurrent.colliderSize;
            // 当たり判定を有効に
            playerProperty.attackCollider.SetActive(true);
        }

        // パリィ成功後のカウントが攻撃可能なフレームの間の場合
        if (playerProperty.skill3AfterParrySuccessCount < playerProperty.atkSObjectCurrent.frameEndCollider &&
            playerProperty.skill3AfterParrySuccessCount >= playerProperty.atkSObjectCurrent.frameStartCollider)
        {
            // 攻撃判定を2フレームごとに点滅
            playerProperty.attackCollider.SetActive( playerProperty.skill3AfterParrySuccessCount%2 == 0);
        }
        // パリィ成功後のカウントがエフェクト生成フレームと同じ場合
        if (playerProperty.skill3AfterParrySuccessCount == playerProperty.atkSObjectCurrent.frameSpawnEffect[playerProperty.atkEffectIndex])
        {
            // 攻撃エフェクトを生成
            var tmp_effect =  MonoBehaviour.Instantiate(playerProperty.atkSObjectCurrent.atkEffect, playerProperty.modelObject.transform);
            // エフェクトの回転を設定
            tmp_effect.transform.localRotation = Quaternion.Euler(playerProperty.atkSObjectCurrent.effectRotation[playerProperty.atkEffectIndex]);
            // エフェクトのサイズを設定
            tmp_effect.transform.localScale = Vector3.one * playerProperty.atkSObjectCurrent.effectSize[playerProperty.atkEffectIndex];
            // 親オブジェクトから独立
            tmp_effect.transform.parent = null;
            // エフェクトの位置を攻撃判定の位置に合わせる
            tmp_effect.transform.position = playerProperty.attackCollider.transform.position;
            // 4秒後エフェクトを削除
            MonoBehaviour.Destroy(tmp_effect , 4f);
            // エフェクトインデックス加算
            playerProperty.atkEffectIndex++;
            // インデックスの最大値を超えたら
            if (playerProperty.atkEffectIndex >= playerProperty.atkSObjectCurrent.frameSpawnEffect.Length)
            {
                // インデックスを0に戻す
                playerProperty.atkEffectIndex = 0;
            }
        }

        // パリィ成功後のカウントが-1より大きい場合 ( 0以上 = パリィ成功)
        if (playerProperty.skill3AfterParrySuccessCount > -1)
        {
            // 加算
            playerProperty.skill3AfterParrySuccessCount++;
        }
        // スキル3カウントが-1より大きい場合 ( 0以上 = パリィできていない)
        if (playerProperty.skill3Count > -1)
        {
            // 加算
            playerProperty.skill3Count++;
        }
        return this;
    }
    public override void ExitEvent()
    {
        // 現在の前方方向を記録
        playerProperty.lastForward = playerProperty.modelObject.transform.forward;
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
        // モデルを表示
        playerProperty.modelObject.SetActive( true);
    }

    public override void TriggerEnterEvent(Collider other)
    {
        // パリィ受付時間内の場合
        if (playerProperty.skill3Count < playerProperty.parryCanCountMax  )
        {
            if (playerProperty.skill3Count != -1)
            {
                // ダメージオブジェクトを取得
                var  tmp_damage = other.GetComponentInParent<IDamageable>();
                // ダメージオブジェクトが存在している場合
                if (tmp_damage != null)
                {
                    // エフェクトを生成し、3秒後削除
                    MonoBehaviour.Destroy(MonoBehaviour.Instantiate(playerProperty.trailEffectPrefab, playerProperty.modelObject.transform.position,Quaternion.identity),3f);
                    // ダメージオブジェクトのパリィされた処理
                    tmp_damage.BeParry(playerProperty);
                    // パリィ成功後のカウントを0に (パリィ成功)
                    playerProperty.skill3AfterParrySuccessCount = 0;
                    // ダメージオブジェクトの位置を記録
                    playerProperty.damagePosition = tmp_damage.Transform.position;
                    // モデルの方向をダメージオブジェクトの位置に向かわせる
                    playerProperty.modelObject.transform
                        .DOLookAt(playerProperty.damagePosition, 0.0f, AxisConstraint.Y);
                    // コントローラーの振動を入れる
                    InputHaptic.instance.Haptic(0,0.5f,2);
                    // 攻撃アニメーションを再生
                    playerProperty.animator.Play(playerProperty.atkSObjectCurrent.animClip.name);
                    // スキル3カウントを停止
                    playerProperty.skill3Count = -1;
                    // パリィ受付を終了
                    playerProperty.parryCanCountMax = 0;
                    // ヒットストップ
                    HitStopManager.instance.HitStop(0.35f,6,true);
                    // SFXを再生
                    AudioClipManager.instance.PlayAudioOneShot("SFX_Skill1");
                    // ボイスを再生
                    AudioClipManager.instance.PlayAudioOneShot("SFX_Skill1Voice");
                }
            }
        }
        // パリィ受付時間を過ぎた場合
        else
        {
            // パリィ成功後のカウントが-1の場合(パリィ失敗）
            if(  playerProperty.skill3AfterParrySuccessCount ==-1)
                // ダメージ処理
                nextState =   PlayerStateSharedMethods.TakeDamage(playerProperty, other, nextState);
        }

    }

}
