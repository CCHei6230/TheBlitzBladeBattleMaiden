using DG.Tweening;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Title : MonoBehaviour
{
    // 言語選択のカーソルのRectTransform
    [SerializeField] RectTransform langCursor;
    // 各言語の位置
    [SerializeField] Vector2[] cursorPositions;

    // ゲームの言語
    [SerializeField]Language language = Language.Japanese;
    [FormerlySerializedAs("IngameScene")]
    public SceneReference InGameScene;
    // プレイヤー入力
    PlayerInputScript playerInputScript;

    // 画面のフェード画像
    [SerializeField] private Image fadeImage;

    void Start()
    {
        //1.05秒後フェードインを完成
        fadeImage.DOFade(0, 1.05f);
        //プレイヤー入力を取得
        playerInputScript = PlayerInputScript.instance;
    }

    void Update()
    {
        if (playerInputScript)
        {
            // 左右ボタンで言語選択
            if (playerInputScript.UI_Left.WasPressedThisFrame())
            {
                var tmp = (int)language-1;
                if (tmp < 0)
                {
                    tmp = 2;
                }

                switch (tmp)
                {
                    case   (int)Language.Japanese:
                        language = Language.Japanese;
                        break;
                    case   (int)Language.Chinese:
                        language = Language.Chinese;
                        break;
                    case   (int)Language.English:
                        language = Language.English;
                        break;
                }
                //選択した言語の位置にカーソルを移動
                langCursor.anchoredPosition = cursorPositions[tmp];
            }
            if (playerInputScript.UI_Right.WasPressedThisFrame())
            {
                var tmp = (int)language+1;
                if (tmp > 2)
                {
                    tmp = 0;
                }

                switch (tmp)
                {
                    case   (int)Language.Japanese:
                        language = Language.Japanese;
                        break;
                    case   (int)Language.Chinese:
                        language = Language.Chinese;
                        break;
                    case   (int)Language.English:
                        language = Language.English;
                        break;
                }
                //選択した言語の位置にカーソルを移動
                langCursor.anchoredPosition = cursorPositions[tmp];
            }
            // 決定ボタンを押した場合
            if (playerInputScript.UI_Submit.WasPressedThisFrame())
            {
                // 入力受付中止
                playerInputScript = null;
                //サウンドを再生
                AudioClipManager.instance.PlayAudioOneShot("SFX_GameStart");
                // 0.5秒後フェードアウトを完成し、更に10フレーム後インゲームシーンに遷移し、このオブジェクトを消す
                fadeImage.DOFade(1, 0.5f).OnComplete(
                        () =>
                        {
                            InGameSceneManager.instance.loadScene(InGameScene, 20);
                            Destroy(gameObject);
                        }
                    );
                //ゲームの言語を設定
                GameLanguage.instance.language = language;
            }
        }
    }
}
