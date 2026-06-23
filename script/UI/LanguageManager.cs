using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ゲームの表示言語を定義する列挙型。
// 0: 日本語、1: 中国語、2: 英語
[Serializable]
public enum Language
{
    Japanese,
    Chinese,
    English
}

// インゲーム画面の UI テキスト・画像を、選択された言語に応じて設定するクラス。
// GameLanguage シングルトンから言語設定と LanguagePackObj を取得して各 UI に反映する。
public class LanguageManager : MonoBehaviour
{
    // 北方向スキルの名前を表示するテキスト UI
    [SerializeField] TMP_Text SkillN;
    // 東方向スキルの名前を表示するテキスト UI
    [SerializeField] TMP_Text SkillE;
    // 南方向スキルの名前を表示するテキスト UI
    [SerializeField] TMP_Text SkillS;
    // 西方向スキルの名前を表示するテキスト UI
    [SerializeField] TMP_Text SkillW;
    // ボスの名前を表示するテキスト UI
    [SerializeField] TMP_Text BossName;
    // コントロールヒント画像 1（言語別の画像を使用）
    [SerializeField] Image ControlHint1;
    // コントロールヒント画像 2（言語別の画像を使用）
    [SerializeField] Image ControlHint2;

    void Start()
    {
        // GameLanguage シングルトンから現在の言語インデックスと対応する LanguagePack を取得する
        var tmp_language = (int)GameLanguage.instance.language;
        var tmp_languagePack = GameLanguage.instance.LanguagePack[tmp_language];
        // 取得した LanguagePack の内容を各 UI に反映する
        SkillN.text = tmp_languagePack.SkillN;
        SkillE.text = tmp_languagePack.SkillE;
        SkillW.text = tmp_languagePack.SkillW;
        SkillS.text = tmp_languagePack.SkillS;
        BossName.text = tmp_languagePack.BossName;
        ControlHint1.sprite = tmp_languagePack.ControlHint1;
        ControlHint2.sprite = tmp_languagePack.ControlHint2;
    }
}
