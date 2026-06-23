using UnityEngine;

[CreateAssetMenu(fileName = "PlayerComboListSObj",
    menuName = "Scriptable Objects/PlayerComboList")]
public class PlayerComboListSObj : ScriptableObject
{
    // 地上通常攻撃のコンボリスト
    public PlayerAttackSObj[] playerNComboList;
    // 空中通常攻撃のコンボリスト
    public PlayerAttackSObj[] playerAirNComboList;
    // 地上ダッシュ攻撃
    public PlayerAttackSObj  playerDashAttack;
    // 空中ダッシュ攻撃
    public PlayerAttackSObj  playerDashAirAttack;
    // 地上強攻撃
    public PlayerAttackSObj  playerHeavyAttack;
    // 空中強攻撃
    public PlayerAttackSObj  playerHeavyAirAttack;
    // スキル攻撃のリスト
    public PlayerAttackSObj[] playerSkillList;
    // 空中スキル攻撃のリスト
    public PlayerAttackSObj[] playerAirSkillList;

}
