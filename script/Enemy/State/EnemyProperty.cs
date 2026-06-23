using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// エネミープロパティの基本クラス
public abstract class EnemyPropertyBase : MonoBehaviour, IAttackable
{
    // モデル
    public GameObject modelObject;
    // 攻撃プレハブ
    public GameObject attackPrefab;
    // 回復アイテムのプレハブ
    [FormerlySerializedAs("attackHealIteamPrefab")]
    public GameObject attackHealItemPrefab;
    // Rigidbody
    public Rigidbody RB;
    // プレイヤーの攻撃情報
    public PlayerAttack playerAttack;
    // 死亡エフェクト
    [FormerlySerializedAs("deatheffectPrefab")]
    public GameObject deathEffectPrefab;
    // 出現エフェクト
    public GameObject spawnEffectPrefab;
    // チャージ完了エフェクト
    public GameObject chargeEndEffectPrefab;
    // HP
    public int hp = 20;
    // 攻撃の経過時間
    public int attackDuration = 35;
    // 攻撃の最大持続時間
    public int attackDurationMax = 35;
    // エネミーグループのマネージャー
    public EnemyGroupManager  groupManager;
    // 出現待機カウント
    public int waitToSpawnCount = 0;

    // モデルのTransformを取得
    public Transform ModleTransform
    {
        get => modelObject.transform;
    }

    // ダメージ処理
    public virtual void DealDamage( PlayerAttack _playerAttack)
    {
        hp -= _playerAttack.damage;
        // プレイヤーの攻撃情報を保存
        playerAttack = _playerAttack;
    }

    // Awake時の初期化
    public virtual StateBase AwakeEvent()
    {
        RB = GetComponent<Rigidbody>();
        return null;
    }
}

// エネミーの種類Enum
[Serializable]
public enum EnemyType
{
    FlyingEyeBall,
    GroundEyeBall,
}

// 各エネミーの具体的なプロパティクラス
public class EnemyProperty : EnemyPropertyBase
{
    // 着地判定のTransform
    public Transform groundCheckTransform = null;
    // 着地判定の距離
    public float groundCheckDistance ;
    // 着地判定のレイヤー
    public LayerMask groundLayer;
    // 出現アニメーション
    public AnimationClip spawnAnimation = null;
    // アイドルアニメーション
    public AnimationClip idleAnimation = null;
    // 移動アニメーション
    public AnimationClip runningAnimation = null;
    // 攻撃アニメーション
    public AnimationClip shootAnimation = null;
    // 落下アニメーション
    public AnimationClip fallAnimation = null;
    // アニメーター
    public Animator animator ;
    // エネミーの種類
    [SerializeField] EnemyType enemyType;
    // stateのDictionary
    public Dictionary<Enum, EnemyStateBase> stateDictionary {  get; private set; }
    // ダメージカウント
    public int damageCount = 0;
    // ダメージカウントの最大値
    public int damageCountMax = 4;
    // プレイヤーとの現在の距離
    [FormerlySerializedAs("detectDistance")] public float currentDistanceToPlayer = 25f;
    // プレイヤーを検知する距離
    [FormerlySerializedAs("distanceToDetect")] public float detectDistanceToPlayer = 50f;
    // 攻撃を開始する距離
    public float distanceToAttack = 10f;
    // 攻撃を維持する最小距離
    public float minimumDistanceToAttack = 15f;
    // 移動速度
    public float speed = 450f;
    // プレイヤーのTransform
    public Transform playerTransform;

    public override StateBase AwakeEvent()
    {
        RB = GetComponent<Rigidbody>();
        switch (enemyType)
        {
            case EnemyType.FlyingEyeBall:
                stateDictionary = new Dictionary<Enum, EnemyStateBase>()
                {
                    { EnemyState.spawn, new EnemyFlyingEyeBallState_SpawnState() },
                    { EnemyState.standby, new EnemyFlyingEyeBallState_StandbyState() },
                    { EnemyState.approach, new EnemyFlyingEyeBallState_ApproachState() },
                    { EnemyState.attack, new EnemyFlyingEyeBallState_AttackState() },
                    { EnemyState.damage, new EnemyFlyingEyeBallState_DamageState() },
                };
                break;
            case EnemyType.GroundEyeBall:
                stateDictionary = new Dictionary<Enum, EnemyStateBase>()
                {
                    { EnemyState.spawn, new EnemyGroundEyeBallState_SpawnState() },
                    { EnemyState.standby, new EnemyGroundEyeBallState_StandbyState() },
                    { EnemyState.approach, new EnemyGroundEyeBallState_ApproachState() },
                    { EnemyState.attack, new EnemyGroundEyeBallState_AttackState() },
                    { EnemyState.damage, new EnemyGroundEyeBallState_DamageState() },
                };
                break;
        }

        foreach (var state in stateDictionary)
        {
            state.Value.SetProperty(this);
        }
        // spawn stateを返す
        return stateDictionary[EnemyState.spawn];
    }
}