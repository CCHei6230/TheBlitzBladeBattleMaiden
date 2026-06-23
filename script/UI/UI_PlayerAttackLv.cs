using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_PlayerAttackLv : MonoBehaviour
{

    // メインUI画像
    [SerializeField] private UI_ImageFillObject fillImage;
    // 背後UI画像
    [FormerlySerializedAs("baseImage")] [SerializeField] private Image backImage;
    // プレイヤー各攻撃レベルの色
    [SerializeField] private Color[] attackLvColors;
    // プレイヤープロパティ
    [SerializeField] PlayerProperty playerProperty;
    void Start()
    {
        backImage.color = attackLvColors[0];
        fillImage.SetColor(attackLvColors[1]);
        fillImage.FillAmount(0 ,1);
    }

    void Update()
    {
        // 攻撃レベルが1より大きい場合
        if (playerProperty.playerAttackLv > 1)
        {
            //メインUI画像のfillAmountを設定
            fillImage.FillAmount(playerProperty.playerAttackLvCount ,playerProperty.playerAttackLv2To4CountMax[Mathf.Clamp(playerProperty.playerAttackLv-2, 0, 3)]);
        }
        backImage.color = attackLvColors[Mathf.Clamp(playerProperty.playerAttackLv-2, 0, 3)];
        fillImage.SetColor(attackLvColors[Mathf.Clamp(playerProperty.playerAttackLv-1, 0, 3)]);
    }
}
