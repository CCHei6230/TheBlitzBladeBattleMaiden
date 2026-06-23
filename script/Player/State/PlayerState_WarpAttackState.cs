using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerState_WarpAttackState : PlayerStateBase
{

    public override void StateEnterEvent()
    {
        nextState = this;
        // ワープ攻撃でのキル数を初期化
        playerProperty.warpAtkKillCount = 0;
        // ワープ攻撃フラグをtrueに
        playerProperty.warpAttacking = true;
        // ワープ攻撃アニメーションを再生
        playerProperty.animator.Play(playerProperty.warpAtkClip.name );
        // アニメーション速度を調整
        playerProperty.animator.speed = 2f;

        // ワープエフェクトを生成し、1秒後削除
        GameObject.Destroy(
            GameObject.Instantiate(playerProperty.warpEffect,
                playerProperty.modelObject.transform.position,
                playerProperty.modelObject.transform.localRotation),1f); 
        
        // 残像エフェクトを生成
        PlayerStateSharedMethods.SpawnMeshTrail(playerProperty.trailObjPool,playerProperty,1, 1, 90);
        
        // ワープ攻撃対象のインデックスを初期化
        playerProperty.warpAtkObjIndex = 0;
        // ワープカウントを設定
        // playerProperty. warpCount =playerProperty. warpCountMax-2;
        playerProperty. warpCount =0;


        // タグしたオブジェクトをリストにコピー
        playerProperty.taggedObject = new List<Transform>();
        for (int i = 0; i < playerProperty.warpAttackManager.taggedObjectList.Count; i++)
        {
            if (playerProperty.warpAttackManager.taggedObjectList[i].transform)
            {
                playerProperty.taggedObject.Add(playerProperty.warpAttackManager.taggedObjectList[i].transform);
            }
        }
      
        // タグリストをクリア
        playerProperty.warpAttackManager.ClearTagObject();
        // ラインレンダラーの設定
        playerProperty.warpAttackManager.lineRenderer.enabled = false;
        playerProperty.warpAttackManager.lineRenderer2.enabled =true;
        playerProperty.warpAttackManager.lineRenderer2.positionCount = 1;
        playerProperty.warpAttackManager.lineRenderer2.SetPosition(playerProperty.warpAttackManager.lineRenderer2.positionCount-1,playerProperty.modelObject.transform.position);
        
        // ヒットストップ
        HitStopManager.instance.HitStop(0.25f,3,true);

        // ランダムにボイスを選択
        int tmp_randomIndex;
        do
        {
            tmp_randomIndex = Random.Range(1, 6);
        } while (tmp_randomIndex == playerProperty.voiceLastIndex);
        playerProperty.voiceLastIndex = tmp_randomIndex;

        // SFXとボイスを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_WarpVoice" + playerProperty.voiceLastIndex);
        AudioClipManager.instance.PlayAudioOneShot("SFX_WarpStart");

    }

    public override StateBase UpdateEvent()
    {
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        // ワープカウント加算
        playerProperty. warpCount++;
    
        // カウント1の時、エフェクトを生成
        if (playerProperty.warpCount == 1)
        {
            GameObject.Destroy(
                GameObject.Instantiate(playerProperty.warpEffect,
                    playerProperty.modelObject.transform.position,
                    playerProperty.modelObject.transform.localRotation),1f);
        }
     
        // ワープ途中の残像生成
        if (playerProperty.warpCount == (int)(playerProperty.warpCountMax / 2) && playerProperty.warpAtkObjIndex  < playerProperty.taggedObject.Count)
        {
            PlayerStateSharedMethods.SpawnMeshTrail(playerProperty.trailObjPool,playerProperty,1, 1, 90);
        }

        // ターゲットが存在する場合のワープ処理
        if (playerProperty.warpAtkObjIndex < playerProperty.taggedObject.Count)
        {
            // カウントが最大値に達した場合
            if (playerProperty.warpCount >= playerProperty.warpCountMax)
            {
                // モデルのDO系処理を中止
                playerProperty.modelObject.transform.DOComplete();

                if (playerProperty.taggedObject[playerProperty.warpAtkObjIndex] )
                {
                    // タグしたオブジェクトを取得
                    ITaggableObject tmp_tagObj =   playerProperty.taggedObject[playerProperty.warpAtkObjIndex].GetComponentInParent<ITaggableObject>();
                    if (tmp_tagObj != null)
                    {
                        // 敵を倒せるか判定(返り値が0以下で判定する為、敵以外のターゲットは対象外)
                        if (tmp_tagObj.HPAfterBeSlashed(playerProperty.playerAttackLv) <= 0)
                        {
                            // 集中線UI演出
                            playerProperty.focuslineUI.SetColor(playerProperty.warpfocuslineUIColor);
                            playerProperty.focuslineUI.DoFadeInOut(0.65f,0.25f);
                            // キル数加算
                            playerProperty. warpAtkKillCount++;
                            // 攻撃レベルを上げる
                            if( playerProperty.warpAtkObjIndex < playerProperty.taggedObject.Count - 1)
                            playerProperty.PlayerAttackLvIncreased();
                        }
                    }

                    // 最後のターゲットの場合、SFXを再生
                    if (playerProperty.warpAtkObjIndex == playerProperty.taggedObject.Count - 1)
                    {
                        AudioClipManager.instance.PlayAudioOneShot("SFX_WarpEnd2");
                    }
                    // コントローラーの振動を入れる
                    InputHaptic.instance.Haptic(0,0.4f,2);

                // ターゲット周辺のランダムな位置にワープ
                Vector3 tmp_randomPostion = Vector3.zero;
                switch (Random.Range(0, 4))
                {
                    case 0:
                        tmp_randomPostion= playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position+playerProperty.taggedObject[playerProperty.warpAtkObjIndex].forward*4f + Vector3.up*1.25f;
                        break;
                    case 1:
                        tmp_randomPostion= playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position+playerProperty.taggedObject[playerProperty.warpAtkObjIndex].right*4f + Vector3.up*1.25f;
                        break;
                    case 2:
                        tmp_randomPostion= playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position+playerProperty.taggedObject[playerProperty.warpAtkObjIndex].right*-4f + Vector3.up*1.25f;
                        break;
                    case 3:
                        tmp_randomPostion= playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position+playerProperty.taggedObject[playerProperty.warpAtkObjIndex].forward*-4f + Vector3.up*1.25f;
                        break;
                }
                playerProperty.RB.position = tmp_randomPostion;

                // 敵の方向に向く
                playerProperty.modelObject.transform
                    .DOLookAt(playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position, 0.0f, AxisConstraint.Y)
                    .OnUpdate(() =>
                        playerProperty.lastForward = playerProperty.modelObject.transform.forward);
                
                // 攻撃アニメーションを再生
                playerProperty.animator.Play(playerProperty.warpAtkClip.name ,0,0);
                
                // 命中エフェクトと斬撃エフェクトを生成
                GameObject.Destroy(GameObject.Instantiate(playerProperty.warpHittedEffect,playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position,Random.rotation),1f); 
                GameObject.Destroy(GameObject.Instantiate(playerProperty.warpSlashEffect,playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position,Random.rotation),0.35f); 
                
                //lineRendererの軌跡を更新
                playerProperty.warpAttackManager.lineRenderer2.positionCount++;
                playerProperty.warpAttackManager.lineRenderer2.SetPosition
                    (playerProperty.warpAttackManager.lineRenderer2.positionCount-1,
                        Vector3.Lerp(playerProperty.warpAttackManager.lineRenderer2.GetPosition(playerProperty.warpAttackManager.lineRenderer2.positionCount-2),
                            playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position,Random.Range(0.25f,0.65f)) + new Vector3(Random.Range(-4f,4f),Random.Range(0f,6f),Random.Range(-4f,4f))
                        )
                ;

                playerProperty.warpAttackManager.lineRenderer2.positionCount++;
                playerProperty.warpAttackManager.lineRenderer2.SetPosition(playerProperty.warpAttackManager.lineRenderer2.positionCount-1,playerProperty.taggedObject[playerProperty.warpAtkObjIndex].position);
                }

                // 速度を0に固定
                playerProperty.RB.linearVelocity = Vector3.zero;
                // カウントをリセット
                playerProperty.warpCount = 0;
                // インデックスを更新
                playerProperty.warpAtkObjIndex++;
                // SFXを再生
                AudioClipManager.instance.PlayAudioOneShot("SFX_Warp2");

            }
        }

        // 全ターゲットのワープが終了した場合
        if(playerProperty.warpAtkObjIndex  == playerProperty.taggedObject.Count)
        {
            // フィニッシュSFXの再生タイミング設定
            // キル数が4以上
            if (playerProperty.warpAtkKillCount >= 4)
            {
                if (playerProperty.warpCount == playerProperty.warpCountMax-15)
                {
                    AudioClipManager.instance.PlayAudioOneShot("SFX_WarpFinish");
                }
            }
            // キル数が4より小さい場合
            else
            {
                if (playerProperty.warpCount == playerProperty.warpCountMax-11)
                {
                    AudioClipManager.instance.PlayAudioOneShot("SFX_WarpFinish");
                }
            }

            // 速度を0に固定
            playerProperty.RB.linearVelocity = Vector3.zero;
            // アニメーション速度を元に戻す
            playerProperty.animator.speed = 1;

            // 4体以上キルした場合の特別演出
            if (playerProperty.warpAtkKillCount >= 4)
            {
                // 演出時間を延長
                playerProperty. warpCountMax = 75;

                // 演出をランダムで再生
                if (playerProperty.warpCount ==10)
                {
                    int tmp_randomIndex;
                    do
                    {
                        tmp_randomIndex = Random.Range(0, playerProperty.warpAttackPlayableAssets.Length );
                    } while (tmp_randomIndex == playerProperty.timelineClipLastIndex);
                    playerProperty. timelineClipLastIndex = tmp_randomIndex;
                    playerProperty.warpAttackTimeline.playableAsset = playerProperty.warpAttackPlayableAssets[playerProperty.timelineClipLastIndex];
                    playerProperty.warpAttackTimeline.Play();
                }

                // 終了時ターゲットへの一斉斬撃
                if (playerProperty.warpCount ==60)
                {
                    for (int i = 0; i < playerProperty.taggedObject.Count; i++)
                    {
                        if (playerProperty.taggedObject[i])
                        {
                            ITaggableObject tmp_tagObj =   playerProperty.taggedObject[i].GetComponentInParent<ITaggableObject>();
                            // ダメージと攻撃レベルアップ判定
                            if(tmp_tagObj.HPAfterBeSlashed(playerProperty.playerAttackLv) <= 0)
                            {
                                if (i == playerProperty.taggedObject.Count - 1)
                                {
                                    playerProperty.PlayerAttackLvIncreased();
                                }
                            }
                            // 斬撃処理
                            tmp_tagObj.BeSlashed(playerProperty);
                            // 斬撃エフェクトとフィニッシュエフェクトを生成
                            GameObject.Destroy(GameObject.Instantiate(playerProperty.warpSlashEffect,playerProperty.taggedObject[i].position,Random.rotation),2f); 
                            GameObject.Destroy(GameObject.Instantiate(playerProperty.warpAtkFinishSlashEffect,playerProperty.taggedObject[i].position,quaternion.identity),2f); 
                        }
                    }
                    // コントローラーを振動させる
                    InputHaptic.instance.Haptic(0.25f,0.4f,2);

                    // lineRendererを非表示に
                    playerProperty.warpAttackManager.lineRenderer2.enabled =false;
                    playerProperty.warpAttackManager.lineRenderer2.positionCount = 0;
                }
            }
            // キル数が少ない場合の演出時間
            else
            {
                playerProperty. warpCountMax = 40;
            }

            // 演出終了後
            if (playerProperty.warpCount >= playerProperty.warpCountMax)
            {
                // 空中攻撃を可能に
                playerProperty.canAirAttack = true;    
                // fall stateに遷移
                nextState = playerProperty.stateDictionary[PlayerState.fall];
            }
        }
        return nextState;
    }

    public override void ExitEvent()
    {
        // ワープ攻撃実行フラグを設定
        playerProperty.hasWarpAtk = true;

        // 攻撃判定をfalseに
        playerProperty.attackCollider.SetActive(false);

        // キル数が4未満の場合の通常斬撃処理
        if (playerProperty.warpAtkKillCount < 4)
        {
            for (int i = 0; i < playerProperty.taggedObject.Count; i++)
            {
                if (playerProperty.taggedObject[i])
                {
                    ITaggableObject tmp_tagObj =   playerProperty.taggedObject[i].GetComponentInParent<ITaggableObject>();
                    if (tmp_tagObj != null)
                    {
                        if(tmp_tagObj.HPAfterBeSlashed(playerProperty.playerAttackLv) <= 0)
                        {
                            if (i == playerProperty.taggedObject.Count - 1)
                            {
                                playerProperty.PlayerAttackLvIncreased();
                            }
                        }
                        // 斬撃処理
                        tmp_tagObj.BeSlashed(playerProperty);

                        // エフェクト生成
                        GameObject.Destroy(GameObject.Instantiate(playerProperty.warpSlashEffect,playerProperty.taggedObject[i].position,Random.rotation),2f); 
                        GameObject.Destroy(GameObject.Instantiate(playerProperty.warpAtkFinishSlashEffect,playerProperty.taggedObject[i].position,quaternion.identity),2f); 
                    }
                }
            }
            // コントローラーを振動させる
            InputHaptic.instance.Haptic(0.25f,0.4f,2);

            // 終了エフェクトを生成
            GameObject.Destroy(GameObject.Instantiate(playerProperty.warpAtkEndEffect,playerProperty.modelObject.transform.position,quaternion.identity),2f); 

            // lineRendererをリセット
            playerProperty.warpAttackManager.lineRenderer2.enabled =false;
            playerProperty.warpAttackManager.lineRenderer2.positionCount = 0;

        }

        // 演出を停止
        playerProperty.warpAttackTimeline.Stop();
        // lineRendererを有効に
        playerProperty.warpAttackManager.lineRenderer.enabled =true;
        
        // タグリストをクリア
        playerProperty.taggedObject.Clear();
        // 二段ジャンプを可能に
        playerProperty.canDoubleJump = true;
        // ダッシュを可能に
        playerProperty.canDash = true;
        // warpCountMaxを7に
        playerProperty.warpCountMax = 7;
        // ワープ攻撃中をfalse
        playerProperty.warpAttacking = false;
    }
}