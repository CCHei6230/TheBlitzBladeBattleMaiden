using UnityEngine;

public class PlayerState_Skill2State : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // 攻撃レベルを下げる
        playerProperty.PlayerAttackLvDecreased();
        // コントローラーの振動を入れる
        InputHaptic.instance.Haptic(0.0f,0.25f,4);
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
        // 着地している場合
        if (playerProperty.isGrounding)
        {
            // 地上スキル2を取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerSkillList[1];
        }
        // 着地していない場合
        else
        {
            // 空中スキル2を取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerAirSkillList[1];
        }
        // エフェクトインデックスを初期化
        playerProperty.atkEffectIndex = 0;
        // 攻撃カウントを0に
        playerProperty.atkCount = 0;
        // 攻撃カウント最大値をスキルのdurationに設定
        playerProperty.atkCountMax =   playerProperty.atkSObjectCurrent.duration;
        // 攻撃レベル上昇の可否を設定
        playerProperty.playerAttack.canIncreaseAttackLv = playerProperty.atkSObjectCurrent.canIncreaseAttackLv;
        // ダメージ量を計算して設定
        playerProperty.playerAttack.damage =playerProperty.atkSObjectCurrent.damage + playerProperty.playerAttackLv*3;
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Skill1");
        // ボイスを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Skill2Voice");
    }

    public override StateBase UpdateEvent()
    {
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
                // 南方向ボタンを押した場合
                if ( playerProperty.playerInput.SkillSlotSInput.WasPerformedThisFrame()&& playerProperty.isGrounding)
                {
                    // skill3 stateに遷移 (落花ノ構え)
                    nextState = playerProperty.stateDictionary[PlayerState.skill3];
                }
            }
        }
        // 移動入力している、かつ移動可能フレームを過ぎた場合
        if (playerProperty.playerInput.moveInputVector!=Vector2.zero && playerProperty.atkCount >playerProperty.atkSObjectCurrent.frameCanMove)
        {
            // move stateに遷移
            nextState =  playerProperty.stateDictionary[PlayerState.move];
        }
        // 攻撃カウントが入力受付期間内の場合
        if ( playerProperty.atkCount >playerProperty.atkSObjectCurrent.frameCanPerformNextMotion &&
             playerProperty.atkCount < playerProperty.atkSObjectCurrent.frameDenyPerformNextMotion )
        {
            // パリィボタンを押したら
            if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
            {
                // parry stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.parry];
            }
        }
        return nextState;
    }



    public override StateBase FixedUpdateEvent()
    {
        // 攻撃カウントが攻撃判定開始フレームと同じ場合
        if (playerProperty.atkCount == playerProperty.atkSObjectCurrent.frameStartCollider)
        {
            // 当たり判定の位置を設定
            playerProperty.attackCollider.transform.localPosition = playerProperty.atkSObjectCurrent.colliderPosition;
            // 当たり判定のサイズを設定
            playerProperty.attackCollider.transform.localScale = playerProperty.atkSObjectCurrent.colliderSize;
            // 当たり判定を有効に
            playerProperty.attackCollider.SetActive(true);
        }
        // 攻撃判定持続中の場合
        if (playerProperty.atkCount < playerProperty.atkSObjectCurrent.frameEndCollider)
        {
            // 攻撃判定を点滅させる
            playerProperty.attackCollider.SetActive( playerProperty.atkCount%2 == 0);
        }
        // 攻撃判定終了フレームの場合
        if (playerProperty.atkCount == playerProperty.atkSObjectCurrent.frameEndCollider)
        {
            // 攻撃判定をfalseに
            playerProperty.attackCollider.SetActive(false);
        }
        // エフェクト生成フレームの場合
        if (playerProperty.atkCount == playerProperty.atkSObjectCurrent.frameSpawnEffect[playerProperty.atkEffectIndex])
        {
            // 攻撃エフェクトを生成
            var tmp_effect =  MonoBehaviour.Instantiate(playerProperty.atkSObjectCurrent.atkEffect, playerProperty.modelObject.transform);
            // エフェクトの回転を設定
            tmp_effect.transform.localRotation = Quaternion.Euler(playerProperty.atkSObjectCurrent.effectRotation[playerProperty.atkEffectIndex]);
            // エフェクトのサイズを設定
            tmp_effect.transform.localScale = Vector3.one * playerProperty.atkSObjectCurrent.effectSize[playerProperty.atkEffectIndex];
            // 親オブジェクトから独立
            tmp_effect.transform.parent = null;
            // 4秒後エフェクトを削除
            MonoBehaviour.Destroy(tmp_effect , 4f);
            // エフェクトインデックス加算
            playerProperty.atkEffectIndex++;
            // インデックスの最大値を超えたら
            if (playerProperty.atkEffectIndex >= playerProperty.atkSObjectCurrent.frameSpawnEffect.Length)
            {
                // 0に戻す
                playerProperty.atkEffectIndex = 0;
            }
        }
        // 攻撃カウント加算
        playerProperty.atkCount++;

        // 突進開始フレームの場合
        if (playerProperty.atkCount == playerProperty.atkSObjectCurrent.frameStartMoveForward)
        {
            // モデルを非表示に
            playerProperty.modelObject.SetActive( false);
            // スキル2のエフェクトを生成
            GameObject.Instantiate(playerProperty.skill2EffectPrefab,
                playerProperty.transform);
            // ワープエフェクトを生成し、1秒後削除
            GameObject.Destroy(
                GameObject.Instantiate(playerProperty.warpEffect,
                    playerProperty.modelObject.transform.position,
                    playerProperty.modelObject.transform.localRotation),1f);
        }
        // 突進終了フレームの場合
        if (playerProperty.atkCount == playerProperty.atkSObjectCurrent.frameEndMoveForward)
        {
            // 突進開始と終了時の着地判定が変わる可能性がある為
            // 着地判定でスキルデータを再設定
            if (playerProperty.isGrounding)
            {
                playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerSkillList[1];
            }
            else
            {
                playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerAirSkillList[1];
            }
            // モデルを表示
            playerProperty.modelObject.SetActive( true);
            // ワープエフェクトを生成し、1秒後削除
            GameObject.Destroy(
                GameObject.Instantiate(playerProperty.warpEffect,
                    playerProperty.modelObject.transform.position,
                    playerProperty.modelObject.transform.localRotation),1f); 
            // スキルアニメーションを再生
            playerProperty.animator.Play(playerProperty.atkSObjectCurrent.animClip.name);
        }

        // 突進中の場合
        if (playerProperty.atkCount > playerProperty.atkSObjectCurrent.frameStartMoveForward
            && playerProperty.atkCount < playerProperty.atkSObjectCurrent.frameEndMoveForward)
        {
            // 移動方向と速度を計算
            Vector3 modelForward = playerProperty.modelObject.transform.forward;
            Vector3 finalVelocity =modelForward * playerProperty.atkSObjectCurrent.forwardSpeed*(Mathf.Abs(1f-((float)playerProperty.atkCount* 0.75f  /(float)playerProperty.atkSObjectCurrent.frameEndMoveForward)))*  Time.fixedDeltaTime;
            // 速度を設定
            playerProperty.RB.linearVelocity = new Vector3(finalVelocity.x,0,finalVelocity.z); 
        }
        else
        {
            // 速度を0に固定
            playerProperty.RB.linearVelocity = Vector3.zero;
        }

        return this;
    }
    public override void ExitEvent()
    {
        // 現在の前方方向を保存
        playerProperty.lastForward = playerProperty.modelObject.transform.forward;
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
        // モデルを表示
        playerProperty.modelObject.SetActive( true);
    }
}
