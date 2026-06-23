using UnityEngine;

// ゲームの表示言語設定を保持するシングルトンクラス。
// シーン遷移後も設定を維持する（DontDestroyOnLoad）。
// LanguageManager や Title などのクラスから参照される。
public class GameLanguage : MonoBehaviour
{
    // 現在設定されているゲームの表示言語
    public Language language = Language.Japanese;
    // 各言語のテキスト・画像データを格納した ScriptableObject の配列
    public LanguagePackObj[] LanguagePack;
    // シングルトンのインスタンス参照
    public static GameLanguage instance;

    void Awake()
    {
        // 既にインスタンスが存在し、かつ別のオブジェクトである場合は自身を破棄する
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // このオブジェクトをシングルトンのインスタンスとして登録する
            instance = this;
            // シーン遷移時にこのオブジェクトを破棄しない
            DontDestroyOnLoad(gameObject);
        }
    }
}
