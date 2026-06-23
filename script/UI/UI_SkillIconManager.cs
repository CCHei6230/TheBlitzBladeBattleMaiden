using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum SkillSlot
{
    N,// 北方向
    E,// 東方向
    S,// 南方向
    W // 西方向
}
public class UI_SkillIconManager : MonoBehaviour
{
    // 北方向のスキル画像
    [SerializeField]Image skillIconNImage;
    // 背景の画像
    [SerializeField]Image skillIconNBackImage;
    // 名前の文字列
    [SerializeField]TMP_Text skillIconNText;

    // 東方向のスキル画像
    [SerializeField]Image skillIconEImage;
    // 背景の画像
    [SerializeField]Image skillIconEBackImage;
    // 名前の文字列
    [SerializeField]TMP_Text skillIconEText;

    // 南方向のスキル画像
    [SerializeField]Image skillIconSImage;
    // 背景の画像
    [SerializeField]Image skillIconSBackImage;
    // 名前の文字列
    [SerializeField]TMP_Text skillIconSText;

    // 西方向のスキル画像
    [SerializeField]Image skillIconWImage;
    // 背景の画像
    [SerializeField]Image skillIconWBackImage;
    // 名前の文字列
    [SerializeField]TMP_Text skillIconWText;

    // スキルアイコン背景の使用できる時の色
    [FormerlySerializedAs("skillAvailable")] [SerializeField]Color skillBackAvailable;
    // スキルアイコンの使用できる時の色
    [SerializeField]Color skillIconAvailable;
    // スキルアイコンのスキルボタン長押し時のスケール
    [SerializeField]float skillIconButtonHoldingScale;
    // スキルUI全体のスキルボタン長押し時のスケール
    [SerializeField]float skillUIObjectButtonHoldingScale;

    // スキルアイコン背景の使用できない時の色
    [FormerlySerializedAs("skillNonAvailable")] [SerializeField]Color skillBackNonAvailable;
    // スキルアイコンの使用できない時の色
    [SerializeField]Color skillIconNonAvailable;
    // スキルアイコンのスキルボタン離す時のスケール
    [SerializeField]float skillIconNonButtonHoldingScale;
    // スキルUI全体のスキルボタン離す時のスケール
    [SerializeField]float skillUIObjectNonButtonHoldingScale;

    // スキルUI全体のオブジェクト
    [SerializeField]GameObject skillUIObject;
    // スキルUIの真ん中のボタンTransform
    [SerializeField]Transform skillCenterObject;

    // プレイヤープロパティ
    [SerializeField] private PlayerProperty playerProperty;

    private void Update()
    {
        SkillHolding(playerProperty.playerInput.SkillInput.IsPressed());
        SkillAvailable(playerProperty.playerAttackLv>1,SkillSlot.N);
        SkillAvailable(playerProperty.canWarpAttack,SkillSlot.E);
        SkillAvailable(playerProperty.playerAttackLv>1 && playerProperty.isGrounding,SkillSlot.S);
        SkillAvailable(playerProperty.playerAttackLv>1,SkillSlot.W);
    }



    // スキルUIをスキルボタンの状態でスケール、不透明度と回転を設定
    public void SkillHolding(bool _skillButtoHolding)
    {
        // スキルボタン長押し時
        if(_skillButtoHolding)
        {
            // スキルUI全体のオブジェクトのスケールがskillUIObjectButtonHoldingScaleと同じではない場合、
            if (skillUIObject.transform.localScale != Vector3.one * skillUIObjectButtonHoldingScale)
            {
                skillUIObject.transform.DOKill();
                skillUIObject.transform.DOScale(Vector3.one * skillUIObjectButtonHoldingScale, 0.075f).
                    OnStart(() =>
                    {
                        skillIconNBackImage.transform.DOScale(Vector3.one * skillIconButtonHoldingScale, 0.075f);
                        skillIconEBackImage.transform.DOScale(Vector3.one * skillIconButtonHoldingScale, 0.075f);
                        skillIconSBackImage.transform.DOScale(Vector3.one * skillIconButtonHoldingScale, 0.075f);
                        skillIconWBackImage.transform.DOScale(Vector3.one * skillIconButtonHoldingScale, 0.075f);
                        skillIconNText.DOFade(1, 0.075f);
                        skillIconEText.DOFade(1, 0.075f);
                        skillIconSText.DOFade(1, 0.075f);
                        skillIconWText.DOFade(1, 0.075f);
                        skillCenterObject.DORotate(new Vector3(0,0,0), 0.075f);
                    });
            }
        }

        // スキルボタン離す時
        else
        {
            // スキルUI全体のオブジェクトのスケールがskillUIObjectNonButtonHoldingScaleと同じではない場合、
            if (skillUIObject.transform.localScale != Vector3.one * skillUIObjectNonButtonHoldingScale)
            {
                skillUIObject.transform.DOKill();
                skillUIObject.transform.DOScale(Vector3.one * skillUIObjectNonButtonHoldingScale, 0.075f).
                OnStart(() =>
                {
                    skillIconNBackImage.transform.DOScale(Vector3.one * skillIconNonButtonHoldingScale, 0.075f);
                    skillIconEBackImage.transform.DOScale(Vector3.one * skillIconNonButtonHoldingScale, 0.075f);
                    skillIconSBackImage.transform.DOScale(Vector3.one * skillIconNonButtonHoldingScale, 0.075f);
                    skillIconWBackImage.transform.DOScale(Vector3.one * skillIconNonButtonHoldingScale, 0.075f);
                    skillIconNText.DOFade(0, 0.075f);
                    skillIconEText.DOFade(0, 0.075f);
                    skillIconSText.DOFade(0, 0.075f);
                    skillIconWText.DOFade(0, 0.075f);
                    skillCenterObject.DORotate(new Vector3(0,0,45), 0.075f);
                });
            }
        }
    }


    // スキルアイコンの色を使用可能の状態で設定
    public void SkillAvailable(bool _available,SkillSlot _slot )
    {
        // 使用可能の場合
        if (_available)
        {
            // スロットで分岐し、使用可能の色に設定
            switch (_slot)
            {
                case SkillSlot.N :
                    skillIconNImage.color = skillIconAvailable;
                    skillIconNBackImage.color = skillBackAvailable;
                    break;
                case SkillSlot.E:
                    skillIconEImage.color = skillIconAvailable;
                    skillIconEBackImage.color = skillBackAvailable;

                    break;
                case SkillSlot.S:
                    skillIconSImage.color = skillIconAvailable;
                    skillIconSBackImage.color = skillBackAvailable;

                    break;
                case SkillSlot.W:
                    skillIconWImage.color = skillIconAvailable;
                    skillIconWBackImage.color = skillBackAvailable;

                    break;
            }
        }
        // 使用不可能の場合
        else
        {
            // スロットで分岐し、使用不可能の色に設定
            switch (_slot)
            {
                case SkillSlot.N :
                    skillIconNImage.color = skillIconNonAvailable;
                    skillIconNBackImage.color = skillBackNonAvailable;
                    break;
                case SkillSlot.E:
                    skillIconEImage.color = skillIconNonAvailable;
                    skillIconEBackImage.color = skillBackNonAvailable;

                    break;
                case SkillSlot.S:
                    skillIconSImage.color = skillIconNonAvailable;
                    skillIconSBackImage.color = skillBackNonAvailable;

                    break;
                case SkillSlot.W:
                    skillIconWImage.color = skillIconNonAvailable;
                    skillIconWBackImage.color = skillBackNonAvailable;

                    break;
            }
        }


    }
}
