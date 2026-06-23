using DG.Tweening;
using UnityEngine;

public class PlayerState_DeathState : PlayerStateBase
{
    public override void StateEnterEvent()
    {
        nextState  = this;
        playerProperty.deathCount = 0;
        // 入力受付を不可に
        playerProperty.playerInput.canInput = false;
        // モデルのDO系処理を中止
        playerProperty.modelObject.transform.DOComplete();
        // ワープ攻撃マネージャーを無効に
        playerProperty.warpAttackManager.enabled = false;
        // HPマネージャーを削除
        MonoBehaviour.Destroy(playerProperty.playerHPManager);
        // 死亡ボイスを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_PlayerDeathVoice");
        // ヒットストップ
        HitStopManager.instance.HitStop(0.25f,3,true);
        // 死亡アニメーションを再生
        playerProperty.animator.CrossFade(playerProperty.deathClip.name,0.05f);
        // タグしたオブジェクトをクリア
        playerProperty.warpAttackManager.ClearTagObject();
    }

    public override StateBase UpdateEvent()
    {
        return nextState;
    }

    public override StateBase FixedUpdateEvent()
    {
        // 死亡カウントが最大値未満の場合
        if (playerProperty.deathCount < playerProperty.deathCountMax)
        {
            playerProperty.deathCount++;
        }

        // 死亡カウントが半分の値に達した場合
        if (playerProperty.deathCount == playerProperty.deathCountMax / 2)
        {
            // モデルを非表示に
            playerProperty.modelObject.gameObject.SetActive(false);
            // ヒットストップ
            HitStopManager.instance.HitStop(0.25f,3,true);
            // 死亡エフェクトを再生
            playerProperty.deathEffectPrefab.Play();
            // コントローラーの振動を入れる
            InputHaptic.instance.Haptic(0.35f,0.0f,3);
            // SFXを再生
            AudioClipManager.instance.PlayAudioOneShot("SFX_PlayerDeath");

            // フェードアウトし、インゲームシーンに遷移
            var tmp_uiFade = UI_FadeScreen.instance;
            if (tmp_uiFade)
            {
                tmp_uiFade.Fade(false , InGameSceneManager.instance.InGameScene);
                // 入力受付を可能に
                playerProperty.playerInput.canInput = true;
            }
        }
        return nextState;
    }
}