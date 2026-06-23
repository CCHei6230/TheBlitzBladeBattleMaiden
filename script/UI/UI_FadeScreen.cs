using UnityEngine;
using DG.Tweening;
using Eflatun.SceneReference;
[DefaultExecutionOrder(-997)]

public class UI_FadeScreen : MonoBehaviour
{
    // クラスの実体
    static public UI_FadeScreen instance = null;
    // フェードイン位置
    [SerializeField] Vector2 inPosition;
    // フェードアウト位置
    [SerializeField] Vector2 outPosition;
    // フェード時間
    [SerializeField] float fadeTime;
    void Awake()
    {
        instance = this;
    }

    // フェード画面処理
    // フェードアウトの場合、フェードアウト後目標シーンに遷移する
    public void Fade(bool _fadeIn , SceneReference _scene =null)
    {
        //フェードイン処理
        if (_fadeIn)
        {
            transform.DOLocalMove(inPosition, 0);
            transform.DOLocalMove(outPosition, fadeTime);
        }
        //フェードアウト処理
        else
        {
            transform.DOLocalMove(outPosition, 0);
            transform.DOLocalMove(inPosition, fadeTime)
                .OnComplete(()=>
                {
                    // 目標シーンに遷移
                    InGameSceneManager.instance.loadScene(_scene,20);
                });
        }
    }
}
