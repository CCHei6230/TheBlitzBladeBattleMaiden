using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_ControllerHint : MonoBehaviour
{
   PlayerInputScript playerInputScript;

   // UI画像
   [SerializeField]Image[] uiImages;
   // UIのTransform
   [SerializeField]Transform uiTransform;

   // フェードインのX座標
   [SerializeField]float XInPosition;
   // フェードアウトのX座標
   [SerializeField] private float XOutPosition;
   // ヒントUIの表示
   bool showHintUI = false;
    void Start()
    {
        playerInputScript = PlayerInputScript.instance;
    }

    void Update()
    {
        // ボタンを押したら
        if (playerInputScript.UI_ControlUI.WasPressedThisFrame())
        {
            // 表示している場合
            if (showHintUI)
            {
                //フェードアウトし、非表示する
                foreach (Image uiImage in uiImages)
                {
                    uiImage.DOComplete();
                    uiImage.DOFade(0, 0.25f);
                }
                uiTransform.DOComplete();
                uiTransform.DOMoveX(XOutPosition, 0.25f);
                showHintUI = false;
            }
            //表示していない場合
            else
            {
                //フェードインし、表示する
                foreach (Image uiImage in uiImages)
                {
                    uiImage.DOComplete();
                    uiImage.DOFade(0.85f, 0.25f);
                }
                uiTransform.DOComplete();
                uiTransform.DOMoveX(XInPosition, 0.25f);
                showHintUI = true;
            }
        }
    }
}
