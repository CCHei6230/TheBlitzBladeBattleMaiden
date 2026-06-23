using DG.Tweening;
using UnityEngine;

// プレイヤーの HP の増減を管理するクラス。
// HP の変化に合わせて HP バーの UI 演出（FillAmount の変化・色フラッシュ）を行う。
public class PlayerHPManager : MonoBehaviour
{
    // プレイヤーのパラメータ・参照を保持するプロパティクラス
    [SerializeField] PlayerProperty playerProperty;

    // HP を減算する処理。HP が 0 を下回らないようにクランプし、UI 演出を再生する。
    public void HPDecreased(int _damage)
    {
        playerProperty.HP -= _damage;

        // HP が 0 を下回った場合は 0 に固定する
        if (playerProperty.HP <= 0)
        {
            playerProperty.HP = 0;
        }

        // HP バーの FillAmount をアニメーションで更新し、赤くフラッシュさせる
        playerProperty.hpImageFillObject.DoFillAmount(playerProperty.HP, playerProperty.HPMax, 0.3f, Ease.OutElastic);
        playerProperty.hpImageFillObject.DoColorToAndBack(Color.red, 0.3f);
    }

    // HP を加算する処理。HP が最大値を超えないようにクランプし、UI 演出を再生する。
    public void HPIncreased(int _amount)
    {
        playerProperty.HP += _amount;
        // HP が最大値を超えた場合は最大値に固定する
        if (playerProperty.HP >= playerProperty.HPMax)
        {
            playerProperty.HP = playerProperty.HPMax;
        }

        // HP バーの FillAmount をアニメーションで更新し、薄緑色にフラッシュさせる
        playerProperty.hpImageFillObject.DoFillAmount(playerProperty.HP, playerProperty.HPMax, 0.1f, Ease.OutExpo);
        playerProperty.hpImageFillObject.DoColorToAndBack(Color.paleGreen, 0.3f);
    }
}
