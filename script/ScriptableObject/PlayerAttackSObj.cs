using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu
    (fileName = "PlayerAttackSObj"
    , menuName = "Scriptable Objects/PlayerAttack")]
public class PlayerAttackSObj : ScriptableObject
{
    // この攻撃で攻撃レベルを上昇させることができるかどうかのフラグ
    public bool canIncreaseAttackLv;
    // 攻撃モーションの総フレーム数
    public int duration;
    // この攻撃から次のモーションへの先行入力が受け付けられるようになるフレーム
    [FormerlySerializedAs("frameCanPreformNextMotion")]
    public int frameCanPerformNextMotion;
    // 次のモーションへの先行入力が受け付けられなくなるフレーム
    [FormerlySerializedAs("frameDenyPreformNextMotion")]
    public int frameDenyPerformNextMotion;
    // プレイヤーの移動入力が有効になるフレーム
    public int frameCanMove;
    // この攻撃の基本ダメージ量
    public int damage;
    // この攻撃で再生するアニメーションクリップ
    public AnimationClip animClip;


    [Header("Effect")]
    // 攻撃エフェクトのプレハブ
    public GameObject atkEffect;
    // 攻撃エフェクトを生成するフレームのリスト（複数のタイミングに対応）
    public int[] frameSpawnEffect;
    // 各エフェクトのサイズ
    public float[] effectSize;
    // 各エフェクトの回転
    public Vector3[] effectRotation;

    [Header("Movement")]
    // 攻撃中の前方への移動速度
    public float forwardSpeed;
    // 攻撃中の上方向への移動速度
    public float risingSpeed;
    // 攻撃中の下方向への移動速度
    public float fallingSpeed;
    // 前方移動を開始するフレーム
    public int frameStartMoveForward;
    // 前方移動を終了するフレーム
    public int frameEndMoveForward;
    // 上方向への移動を開始するフレーム
    public int frameStartMoveUp;
    // 上方向への移動を終了するフレーム
    public int frameEndMoveUp;
    // 下方向への移動を開始するフレーム
    public int frameStartMoveDown;
    // 下方向への移動を終了するフレーム
    public int frameEndMoveDown;

    [Header("Collider")]
    // 攻撃コライダーのサイズ
    public Vector3 colliderSize;
    // 攻撃コライダーのローカル座標上の位置
    [FormerlySerializedAs("colliderPosistion")]
    public Vector3 colliderPosition;
    // 攻撃コライダーを有効にするフレーム
    public int frameStartCollider;
    // 攻撃コライダーを無効にするフレーム
    public int frameEndCollider;
}
