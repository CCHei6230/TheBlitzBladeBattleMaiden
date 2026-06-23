using DG.Tweening;
using UnityEngine;

public class PlayerState_ParrySuccessState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState = this;
        // カウントを0に
        playerProperty.parryCount = 0;
        // 最大値を23に
        playerProperty.parryCountMax = 23;
        // コントローラーの振動を入れる
        InputHaptic.instance.Haptic(0,0.5f,2);

        // 着地判定に基づいてアニメーション再生
        if (playerProperty.isGrounding)
        {
            playerProperty.animator.CrossFade(playerProperty.parrySuccessClip.name,0);
        }
        else
        {
            playerProperty.animator.CrossFade(playerProperty.parrySuccessAirClip.name,0);
        }
        // 移動入力に応じた回転処理
        if (playerProperty.playerInput.moveInputVector != Vector2.zero)
        {
            var tmp_cameraTransform = playerProperty.cameraTransform;
            var cameraForward = new Vector3(tmp_cameraTransform.forward.x,0, tmp_cameraTransform.forward.z).normalized;
            var cameraRight = new Vector3(playerProperty.cameraTransform.right.x,0, tmp_cameraTransform.right.z).normalized;
            playerProperty.lastForward = (cameraForward*playerProperty.playerInput.moveInputVector.y + cameraRight*playerProperty.playerInput. moveInputVector.x);
        }
        else
        {
            playerProperty.lastForward = -(playerProperty.modelObject.transform.forward);
        }
        // 逆方向に向く（後退移動の演出）
        playerProperty.modelObject.transform.DORotate(Quaternion.LookRotation(-playerProperty.lastForward).eulerAngles,0f);
        // 演出エフェクトと残像を生成
        PlayerStateSharedMethods.SpawnMeshTrail(playerProperty.trailObjPool,playerProperty, 15, 1, 15);
        // ヒットストップ
        HitStopManager.instance.HitStop(0.35f,4,true);
        // 残像エフェクトを生成
        MonoBehaviour.Destroy(MonoBehaviour.Instantiate(playerProperty.trailEffectPrefab, playerProperty.modelObject.transform.position,Quaternion.identity),3f);
        // 攻撃レベルを上げる
        playerProperty.PlayerAttackLvIncreased();
        // 集中線UIの色を設定し、表示する
        playerProperty.focuslineUI.SetColor(playerProperty.parryfocuslineUIColor);
        playerProperty.focuslineUI.DoFadeInOut(0.85f,0.07f);
        // パリィ時の前進方向を記録
        playerProperty.parryForwardVector = playerProperty.lastForward.normalized;
        // SFXを再生 
        AudioClipManager.instance.PlayAudioOneShot("SFX_Parry");
    }

    public override StateBase UpdateEvent()
    {
        // 移動入力がある場合、方向を更新
        if (playerProperty.playerInput.moveInputVector != Vector2.zero)
        {
            var tmp_cameraTransform = playerProperty.cameraTransform;
            var cameraForward = new Vector3(tmp_cameraTransform.forward.x,0, tmp_cameraTransform.forward.z).normalized;
            var cameraRight = new Vector3(playerProperty.cameraTransform.right.x,0, tmp_cameraTransform.right.z).normalized;
            playerProperty.lastForward = (cameraForward*playerProperty.playerInput.moveInputVector.y + cameraRight*playerProperty.playerInput. moveInputVector.x);
            playerProperty.modelObject.transform.DORotate(Quaternion.LookRotation(playerProperty.lastForward).eulerAngles,0);
            playerProperty.parryForwardVector = playerProperty.lastForward.normalized;
        }
        // キャンセル可能期間
        if (playerProperty.parryCount > 5)
        {
            // ダメージボタンを押した場合
            if (playerProperty.playerInput.dashInput.WasPressedThisFrame())
            {
                // 空中かつダッシュ可能な場合
                if (!playerProperty.isGrounding && playerProperty.canDash)
                {
                    // ダッシュ可能をfalseに
                    playerProperty.canDash = false;
                    // dash stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.dash];
                }
                // 着地中
                else if (playerProperty.isGrounding)
                {
                    // dash stateに遷移
                    nextState = playerProperty.stateDictionary[PlayerState.dash];
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

            // 強攻撃ボタンを押した場合
            if (playerProperty.playerInput.heavyAttackInput.WasPressedThisFrame())
            {
                // 着地中
                if (playerProperty.isGrounding)
                {
                    // heavyAttack state に遷移
                    nextState = playerProperty.stateDictionary[PlayerState.heavyAttack];
                }
                // 空中
                else
                {
                    // heavyAttackAir state に遷移
                    nextState = playerProperty.stateDictionary[PlayerState.heavyAttackAir];
                }
            }

            if (playerProperty.playerInput.ParryInput.WasPressedThisFrame())
            {
                // parry state に遷移
                nextState = playerProperty.stateDictionary[PlayerState.parry];
            }
        }

        // カウントが最大値-5以上
        if (playerProperty.parryCount >= playerProperty.parryCountMax-5)
        {
            // 移動入力がある場合
            if (playerProperty.playerInput.moveInputVector != Vector2.zero)
            {
                // move state に遷移
                nextState = playerProperty.stateDictionary[PlayerState.move];
            }
        }
        // カウントが最大値以上
        if (playerProperty.parryCount >= playerProperty.parryCountMax)
        {
            // 着地中
            if (playerProperty.isGrounding)
            {
                // idle state に遷移
                nextState = playerProperty.stateDictionary[PlayerState.idle];
            }
            // 空中
            else
            {
                // fall state に遷移
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
        // パリィ成功後の後退移動
        playerProperty.RB.linearVelocity
            = playerProperty.parryForwardVector * Time.fixedDeltaTime *
              (((1 - ((float)playerProperty.parryCount / (float)playerProperty.parryCountMax)) * playerProperty.dashSpeed  ));
        return this;
    }
    public override void ExitEvent()
    {
        // 前方方向を保存
        playerProperty.lastForward = (playerProperty.modelObject.transform.forward);
        // パリィ時の前方方向を0に
        playerProperty.parryForwardVector = Vector3.zero;
    }
}
