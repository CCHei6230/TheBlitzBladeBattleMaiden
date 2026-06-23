using UnityEngine;

[CreateAssetMenu
    (fileName = "LanguagePackObj"
    , menuName = "Scriptable Objects/Language")]
public class LanguagePackObj : ScriptableObject
{
    // 北方向スキルの名前
    public string SkillN;
    // 東方向スキルの名前
    public string SkillE;
    // 南方向スキルの名前
    public string SkillS;
    // 西方向スキルの名前
    public string SkillW;
    // ボスの名前
    public string BossName;
    // コントロールヒント1の画像
    public Sprite ControlHint1;
    // コントロールヒント2の画像
    public Sprite ControlHint2;
}
