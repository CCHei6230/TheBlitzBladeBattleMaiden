using UnityEngine;
using DG.Tweening;

public class PlayerState_Skill1State : PlayerStateBase
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
            // 地上スキルを取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerSkillList[0];
        }
        // 着地していない場合
        else
        {
            // 空中スキルを取得
            playerProperty.atkSObjectCurrent = playerProperty.playerComboList.playerAirSkillList[0];
        }
        // エフェクトインデックスを初期化
        playerProperty.atkEffectIndex = 0;
        // 攻撃カウントを0に
        playerProperty.atkCount = 0;
        // 攻撃カウント最大値をスキルのdurationに設定
        playerProperty.atkCountMax = playerProperty.atkSObjectCurrent.duration;
        // 先行入力可能フレームを設定
        playerProperty.atkSObjectCurrent.frameCanPerformNextMotion  = playerProperty.atkSObjectCurrent.frameCanPerformNextMotion;
        // 攻撃レベル上昇の可否を設定
        playerProperty.playerAttack.canIncreaseAttackLv = playerProperty.atkSObjectCurrent.canIncreaseAttackLv;
        // ダメージ量を計算して設定
        playerProperty.playerAttack.damage =playerProperty.atkSObjectCurrent.damage +playerProperty.playerAttackLv;
        // スキルアニメーションを再生
        playerProperty.animator.Play(playerProperty.atkSObjectCurrent.animClip.name);
        // SFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Skill1");
        // ボイスを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Skill1Voice");
        // 残像エフェクトを生成
        PlayerStateSharedMethods.SpawnMeshTrail(playerProperty.trailObjPool,playerProperty,15, 1, 25);
    }

    public override StateBase UpdateEvent()
    {
        // パリィボタンを押したら
        if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
        {
            // parry stateに遷移
            nextState = playerProperty.stateDictionary[PlayerState.parry];
        }
        // 攻撃終了後の遷移
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
            if (playerProperty.playerAttackLv > 1)
            {
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
        // 移動入力している、かつ移動可能フレームを過ぎた場合
        if (playerProperty.playerInput.moveInputVector!=Vector2.zero && playerProperty.atkCount >playerProperty.atkSObjectCurrent.frameCanMove)
        {
            // move stateに遷移
            nextState =  playerProperty.stateDictionary[PlayerState.move];
        }

        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        // 攻撃判定開始フレームの場合
        if (playerProperty.atkCount == playerProperty.atkSObjectCurrent.frameStartCollider)
        {
            // 当たり判定の位置を設定
            playerProperty.attackCollider.transform.localPosition = playerProperty.atkSObjectCurrent.colliderPosition;
            // 当たり判定のサイズを設定
            playerProperty.attackCollider.transform.localScale = playerProperty.atkSObjectCurrent.colliderSize;
            // 当たり判定を有効に
            playerProperty.attackCollider.SetActive(true);
            
            // スキル1のエフェクトを生成し、演出を入れる
            var tmp_effect =
                GameObject.Instantiate(playerProperty.skill1EffectPrefab,playerProperty.attackCollider.transform.position,Quaternion.identity);
            // スケーリングアップ演出
            tmp_effect.transform.DOScale(Vector3.one * 25, 0.5f).SetEase(Ease.OutBack)
                .OnStart(() =>
                {
                    // マテリアルの_power演出
                    var tmp_render = tmp_effect.GetComponentsInChildren<Renderer>();
                    foreach (var render in tmp_render)
                    {
                        render.material.DOFloat(-3.5f, "_power",0.5f);
                    }
                })
                .OnComplete(
                    () => {
                        // スケーリングダウン演出
                        tmp_effect.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutExpo)
                        .OnStart(() =>
                        {
                            // マテリアルの_power演出
                            var tmp_render = tmp_effect.GetComponentsInChildren<Renderer>();
                            foreach (var render in tmp_render)
                            {
                                render.material.DOFloat(0, "_power",0.5f);
                            }
                        })
                        // 演出後エフェクトを削除
                        .OnComplete(()=> { GameObject.Destroy(tmp_effect); })
                        ; })
                ;
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
            // エフェクトの位置を設定
            tmp_effect.transform.position =  playerProperty.attackCollider.transform.position;
            // 親要素を解除
            tmp_effect.transform.parent = null;
            // 4秒後エフェクトを削除
            MonoBehaviour.Destroy(tmp_effect , 4f);
            
            // エフェクトインデックス加算
            playerProperty.atkEffectIndex++;
            if (playerProperty.atkEffectIndex >= playerProperty.atkSObjectCurrent.frameSpawnEffect.Length)
            {
                playerProperty.atkEffectIndex = 0;
            }
        }

        // 攻撃カウントを更新
        playerProperty.atkCount++;

        // 後方移動
        if (playerProperty.atkCount > playerProperty.atkSObjectCurrent.frameStartMoveForward
            && playerProperty.atkCount < playerProperty.atkSObjectCurrent.frameEndMoveForward)
        {
            // 移動方向と速度を計算
            Vector3 modelForward = playerProperty.modelObject.transform.forward;
            Vector3 finalVelocity = modelForward * playerProperty.atkSObjectCurrent.forwardSpeed*(1-(playerProperty.atkCount /playerProperty.atkCountMax-10));
            // 速度を適用
            playerProperty.RB.linearVelocity = new Vector3(finalVelocity.x,0,finalVelocity.z); 
        }
        // 移動停止
        else
        {
            playerProperty.RB.linearVelocity = Vector3.zero;
        }

        return this;
    }

    public override void ExitEvent()
    {
        // カウントを0に
        playerProperty.atkCount = 0;
        // 最大値を0に
        playerProperty.atkCountMax = 0;
        // エフェクトインデックスを0に
        playerProperty.atkEffectIndex = 0;
        // 現在の前方方向を保存
        playerProperty.lastForward = playerProperty.modelObject.transform.forward;
        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);
    }
}