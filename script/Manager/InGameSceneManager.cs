using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;
using UnityEngine.Serialization;

// インゲームのシーン遷移を管理するシングルトンクラス。
// 指定したフレーム数の待機後にシーンをロードする非同期処理を提供する。
// シーン遷移後もオブジェクトが保持される（DontDestroyOnLoad）。
public class InGameSceneManager : MonoBehaviour
{
    // シングルトンのインスタンス参照
    public static InGameSceneManager instance;
    // インゲームシーンの参照
    public SceneReference InGameScene;
    // タイトルシーンの参照
    [FormerlySerializedAs("TilteScene")]
    public SceneReference TitleScene;

    private void Awake()
    {
        // シングルトンパターン：既存のインスタンスがあれば自身を破棄する
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // 指定したフレーム数の待機後、指定したシーンへ遷移する処理。
    // _scene : 遷移先のシーン参照
    // _time  : 待機フレーム数（60FPS 基準）
    public void loadScene(SceneReference _scene, int _time)
    {
        StartCoroutine(IEnumerator_loadSceneRef(_scene, _time));
    }

    // シーン遷移の非同期処理本体。指定フレーム数待機後にシーンをロードする。
    public IEnumerator IEnumerator_loadSceneRef(SceneReference _scene, int _time)
    {
        // 指定フレーム数待機する
        for (int i = 0; i < _time; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        // 指定したシーンをロードする
        SceneManager.LoadScene(_scene.Name);
        // シーン遷移時に親オブジェクトごと削除されるのを防ぐため、親から切り離す
        gameObject.transform.parent = null;
    }
}
