using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_TagObjectManager : MonoBehaviour
{
    [SerializeField]Image[] tagImage;
    [SerializeField]Image[] tagScopeImage;
    [SerializeField]Image[] tagImageForCount;
    [SerializeField]UI_ImageFillObject[] focusImagesFill;
    [SerializeField]Color focusRefillColor;
    [SerializeField]Color focusColor;
    [SerializeField]PlayerWarpAttackManager playerWarpAttackManager;
    Camera camera;
    void Start()
    {
        camera=Camera.main;
        for (int i = 0; i < tagImage.Length; i++)
        {
            tagImage[i].transform.DOScale(Vector3.one * 0.6f, 0);
            tagImage[i].transform.DORotate(Vector3.zero, 0);
            tagImage[i].gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        //集中ゲージの色とFillAmountを設定
        for (int i = 0; i < focusImagesFill.Length; i++)
        {
            // 強制リロードしている場合
            if (playerWarpAttackManager.playerProperty.Coroutine_FocusReload != null)
            {
                focusImagesFill[i].SetColor(focusRefillColor);
            }
            // 強制リロードしていない場合
            else
            {
                focusImagesFill[i].SetColor(focusColor);
            }
            focusImagesFill[i].FillAmount(playerWarpAttackManager.playerProperty.focusCount,playerWarpAttackManager.playerProperty.focusCountMax);
        }

        //タグしたオブジェクトの分、タグUIを表示する
        for (int i = 0; i < tagScopeImage.Length ; i++)
        {
            tagScopeImage[i].enabled = false;
        }
        for (int i = 0; i < tagScopeImage.Length || i <playerWarpAttackManager.scopeTaggedList.Count; i++)
        {
            if (i >= playerWarpAttackManager.scopeTaggedList.Count)
            {
                break;
            }

            if (playerWarpAttackManager.scopeTaggedList[i])
            {
                tagScopeImage[i].enabled = true;
            }
        }

        for (int i = 0; i < playerWarpAttackManager.taggedObjectList.Count; i++)
        {
            //タグ開始の演出
            if (playerWarpAttackManager.taggedObjectList[i].taggedCount == playerWarpAttackManager.taggedCountMax )
            {
                //tagImageのサイズと回転と透明度をリセット
                tagImage[i].gameObject.SetActive(true);
                tagImage[i].DOComplete();
                tagImage[i].transform.DORotate(Vector3.zero, 0);
                tagImage[i].transform.DOScale(Vector3.one * 0.6f, 0);
                tagImage[i].DOFade(0, 0);
                //tagImageをイースで拡大、回転とフェイドイン
                tagImage[i].transform.DOScale(Vector3.one * 0.2f, 0.3f).SetEase(Ease.OutBack);
                tagImage[i].DOFade(0.75f, 0.2f);
                tagImage[i].transform.DORotate(new Vector3(0, 0, 45f), 0.25f).SetEase(Ease.OutCirc);
            }
            //fillAmountを設定
            tagImageForCount[i].fillAmount = (float)playerWarpAttackManager.taggedObjectList[i].taggedCount/(float)playerWarpAttackManager.taggedCountMax;
        }
    }

    void Update()
    {
        for (int i = 0; i < tagImage.Length; i++)
        {
                // taggedCountが-1より大き、
                // かつtransformが存在している場合
                if (playerWarpAttackManager.taggedObjectList[i].taggedCount > -1 && playerWarpAttackManager.taggedObjectList[i].transform)
                {
                    // カメラを基準に、画面上タグUIの位置を設定
                    tagImage[i].transform.position = camera.WorldToScreenPoint(playerWarpAttackManager.taggedObjectList[i].transform.position);
                    //タグしたオブジェクトが画面内に存在しているかでタグUIの表示を設定
                    tagImage[i].gameObject.SetActive(playerWarpAttackManager.taggedObjectList[i].renderer.isVisible);
                    //タグ開始後、タグUIを縮小させる
                    if (tagImage[i].gameObject &&tagImage[i].transform.localScale != Vector3.one * 0.2f)
                    {
                        tagImage[i].DOComplete();
                        tagImage[i].transform.DOScale(Vector3.one * 0.2f,0.3f).SetEase(Ease.OutBack);
                        tagImage[i].DOFade(0.75f, 0.2f);
                    }
                }
                // それ以外
                else
                {
                    //タグUIを非表示する
                    tagImage[i].transform.DOScale(Vector3.one* 0.3f, 0.0f);
                    tagImage[i].DOFade(0, 0.0f);
                    tagImage[i].gameObject.SetActive(false);
                }
        }
    }
}
