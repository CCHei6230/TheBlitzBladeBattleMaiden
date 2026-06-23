using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

// プレイヤーステートEnum
public enum PlayerState
{
    idle,
    attack,
    warpAttack,
    move,
    jump,
    fall,
    dash,
    dashAttack,
    heavyAttack,
    heavyAttackAir,
    heavyAttackGround,
    damage,
    death,
    parry,
    parrySuccess,
    skill1,
    skill2,
    skill3,
}
[DefaultExecutionOrder(-10)]

public class PlayerProperty : MonoBehaviour
{
    // 経過時間を計るカウント
    [Header("Count")]
    // 攻撃
    public int atkCount = 0;
    // ジャンプ
    public int jumpCount = 0;
    // ダッシュ
    public int dashCount = 0;
    // 空中強攻撃からの着地
    public int heavyAttackAirGroundCount = 0;
    // パリィ
    public int parryCount;
    // ワープ攻撃
    public int warpCount = 0;
    // スキル3
    public int skill3Count;
    // スキル3中パリィ成功後のカウント
    public int skill3AfterParrySuccessCount;
    // ダメージ
    public int damageCount = 0;
    // 死亡
    [FormerlySerializedAs("deathCout")]
    public int deathCount;

    // カウントの最大値
    [Header("CountMax")]
    // 攻撃
    public int atkCountMax = 0;
    // ジャンプ
    public int jumpCountMax = 20;
    // ダッシュ
    public int dashCountMax = 15;
    // 空中強攻撃からの着地
    public int heavyAttackAirGroundCountMax = 30;
    // パリィモーション全体の経過時間
    public int parryCountMax;
    // パリィ可能な最大フレーム
    public int parryCanCountMax;
    // ワープ攻撃
    public int warpCountMax = 7;
    // スキル3(パリィ失敗)
    public int skill3CountMax = 95;
    // スキル3(パリィ成功からの追加攻撃)
    public int skill3AtkCountMax ;
    // ダメージ
    public int damageCountMax = 60;
    // 死亡
    [FormerlySerializedAs("deathCoutMax")]
    public int deathCountMax = 70;

    [Header("HP")]
    // プレイヤーのHP
    public int HP = 100;
    // 最大値
    public int HPMax = 100;

    [Header("Move")]
    // 移動速度
    public float moveSpeed = 500;
    // 移動方向
    public Vector3 moveDirection = Vector3.zero;
    // 前の前方方向
    public Vector3 lastForward;

    [Header("Ground Check")]
    // 着地しているか
    public bool isGrounding = false;
    // 着地判定の距離
    public float groundCheckDistance = 0.25f;
    // 着地判定のTransform
    public Transform groundCheckTransform;
    // 着地判定の目標レイヤー
    public LayerMask groundLayer;

    [Header("Jump")]
    // ワープ攻撃したか
    public bool hasWarpAtk = false;
    // 二段ジャンプ可能か
    public bool canDoubleJump = true;
    // ジャンプ力
    public float jumpForce = 50f;
    // 低重力
    public float lowGravityRate = 1.5f;
    // 二段ジャンプエフェクト
    public ParticleSystem jumpWaveParticles;

    [Header("Dash")]
    // ダッシュ可能か
    public bool canDash = true;
    // ダッシュ速度
    public float dashSpeed = 2500;
    // ダッシュエフェクト
    public ParticleSystem dashWaveParticles;

    [Header("Parry")]
    // 空中パリィ可能か
    public bool  canAirParry = true;
    // ダメージ時のパリィしたフレーム
    public int parryFrameWhileDamage;
    // パリィ可能なオブジェクト
    public IDamageable objectCanParry;
    // パリィ時の前方方向
    [HideInInspector]public Vector3 parryForwardVector;

    [Header("Damage")]
    // 受けたダメージ
    public int takeDamage = 0;
    // ダメージオブジェクトの位置
    public Vector3 damagePosition;
    // ダメージエフェクト
    public GameObject damageEffectPrefab;
    // 死亡エフェクト
    public ParticleSystem deathEffectPrefab;

    [Header("Attack")]
    // 現一般攻撃のコンボ
    public int atkCombo = 0;
    // 現攻撃のエフェクトインデックス
    public int atkEffectIndex;
    // 攻撃入力受付可能フラグ
    public bool atkPreInput;
    // 空中攻撃
    public bool canAirAttack = true;
    // 攻撃時の前進速度
    public float atkForwardSpeed = 0;
    // 現攻撃のPlayerAttackSObj
    public PlayerAttackSObj atkSObjectCurrent;
    // PlayerAttackSObjを管理するリスト
    public PlayerComboListSObj playerComboList;
    // 攻撃判定のクラス
    public PlayerAttack playerAttack;
    // 攻撃のコライダー
    public GameObject attackCollider;
    // ヒットエフェクト
    public GameObject hitEffect;
    // 強攻撃のエフェクト
    public ParticleSystem heavyAttackWaveParticles;

    [Header("Attack Level")]
    // 攻撃レベル
    [Range(1,4)] public int playerAttackLv = 1;
    // 攻撃レベルのカウント(0になったらレベルが1下がる)
    public int playerAttackLvCount = 0;
    // 各レベルのカウントの最大値
    public int[] playerAttackLv2To4CountMax = {  2100, 1500, 600 };
    // 刀のモデル
    public SkinnedMeshRenderer katanaModel;
    // 各レベルの刀のマテリアル
    public Material[] playerAttackLvMaterials;

    [Header("WarpAttack")]
    // 集中ゲージ
    public int focusCount = 90;
    // 集中ゲージの最大値
    public int focusCountMax = 90;
    // ワープ攻撃対象のインデックス
    public int warpAtkObjIndex = 0;
    // 一回のワープ攻撃で倒した敵のキル数
    public int warpAtkKillCount = 0;
    // タグしたオブジェクトリスト
    public List<Transform> taggedObject;
    // 前のワープ攻撃の演出クリップインデックス
    public int timelineClipLastIndex = 0;
    // 前のワープ攻撃のボイスインデックス
    public int voiceLastIndex = 0;
    // ワープ攻撃中
    public bool warpAttacking = false;
    // ワープ攻撃可能か
    public bool canWarpAttack;
    // ワープ攻撃斬撃エフェクト
    public GameObject  warpSlashEffect;
    // ワープ攻撃命中エフェクト
    public GameObject  warpHittedEffect;
    // ワープエフェクト
    public GameObject  warpEffect;
    // ワープ攻撃終了の斬撃エフェクト
    public GameObject  warpAtkFinishSlashEffect;
    // ワープ攻撃終了エフェクト
    public GameObject  warpAtkEndEffect;
    // ワープ攻撃の演出の再生用
    public PlayableDirector warpAttackTimeline;
    // ワープ攻撃の演出クリップリスト
    public PlayableAsset[] warpAttackPlayableAssets;

    [Header("Skill")]
    // スキル1のエフェクト
    public GameObject skill1EffectPrefab;
    // スキル2のエフェクト
    public GameObject skill2EffectPrefab;
    // スキル3のエフェクト
    public GameObject skill3EffectPrefab;

    [Header("UI")]
    // 集中モードUIのTransform
    public Transform scopeUI ;
    // HPのUI画像
    public UI_ImageFillObject hpImageFillObject;
    // 集中線UI
    public UI_ImageFillObject focuslineUI;
    // ワープ攻撃の集中線の色
    public Color warpfocuslineUIColor;
    // ダメージの集中線の色
    public Color damagefocuslineUIColor;
    // パリィ成功の集中線の色
    public Color parryfocuslineUIColor;

    [Header("Animation")]
    // アニメーター
    public Animator animator;
    // アイドル
    public AnimationClip idleClip;
    // 移動
    public AnimationClip moveClip;
    // ジャンプ
    public AnimationClip jumpClip;
    // 二段ジャンプ
    public AnimationClip doubleJumpClip;
    // 落下
    public AnimationClip fallClip;
    // ダッシュ
    public AnimationClip dashClip;
    // 空中ダッシュ
    public AnimationClip dashAirClip;
    // ダメージ
    public AnimationClip damageClip;
    // 空中ダメージ
    public AnimationClip damageAirClip;
    // 死亡
    public AnimationClip deathClip;
    // 空中強攻撃からの着地
    public AnimationClip heavyAttackAirGroundClip ;
    // ワープ攻撃
    public AnimationClip warpAtkClip;
    // パリィ
    public AnimationClip parryClip;
    // 空中パリィ
    public AnimationClip parryAirClip;
    // パリィ成功
    public AnimationClip parrySuccessClip;
    // 空中パリィ成功
    public AnimationClip parrySuccessAirClip;
    // スキル3
    public AnimationClip skill3Clip;

    [Header("Other")]
    // Rigidbody
    public Rigidbody RB = null;
    // モデル
    public GameObject modelObject;
    // カメラのTransform
    public Transform cameraTransform;
    // ロックオンしている敵
    [FormerlySerializedAs("lockOnEenemy")]
    public Transform  lockOnEnemy;
    // プレイヤー入力
    public PlayerInputScript playerInput;
    // HPマネージャー
    public PlayerHPManager playerHPManager;
    // ワープ攻撃マネージャー
    public PlayerWarpAttackManager warpAttackManager;
    // 敵のレイヤー
    public LayerMask enemyLayer;
    // 残像マテリアル
    public Material trailMaterial;
    // 残像のオブジェクトプール
    public ObjectPool trailObjPool;
    // 残像エフェクト
    public GameObject trailEffectPrefab;
    // 集中ゲージ強制リロードコルーチン
    public Coroutine Coroutine_FocusReload = null;
    // 集中ゲージ回復コルーチン
    public Coroutine Coroutine_FocusFilling = null;
    // stateのDictionary
    public Dictionary<PlayerState, PlayerStateBase> stateDictionary {  get; private set; }

    public void AwakeEvent()
    {
        //プレイヤー入力を設定
        playerInput = PlayerInputScript.instance;
        //Rigidbodyを取得
        RB = GetComponent<Rigidbody>();
        //プレイヤー攻撃のplayerPropertyを設定
        playerAttack.playerProperty = this;
        //プレイヤー攻撃の判定をfalseに
        attackCollider.SetActive(false);

        //全PlayerStateを作成
        stateDictionary = new Dictionary<PlayerState, PlayerStateBase>(){
            {PlayerState.idle, new PlayerState_IdleState()},
            {PlayerState.move, new PlayerState_MoveState()},
            {PlayerState.dash, new PlayerState_DashState()},
            {PlayerState.dashAttack, new PlayerState_DashAttackState()},
            {PlayerState.jump, new PlayerState_JumpState()},
            {PlayerState.fall, new PlayerState_FallState()},
            {PlayerState.warpAttack, new PlayerState_WarpAttackState()},
            {PlayerState.damage, new PlayerState_DamageState()},
            {PlayerState.death, new PlayerState_DeathState()},
            {PlayerState.attack, new PlayerState_AttackState()},
            {PlayerState.heavyAttackAir, new PlayerState_HeavyAttackAirState()},
            {PlayerState.heavyAttack, new PlayerState_HeavyAttackState()},
            {PlayerState.heavyAttackGround, new PlayerState_HeavyAttackAirGroundState()},
            {PlayerState.parry, new PlayerState_ParryState()},
            {PlayerState.parrySuccess, new PlayerState_ParrySuccessState()},
            {PlayerState.skill1, new PlayerState_Skill1State()},
            {PlayerState.skill2, new PlayerState_Skill2State()},
            {PlayerState.skill3, new PlayerState_Skill3State()},
        };
        stateDictionary[PlayerState.idle].SetProperty(this);
        HP = HPMax;
    }
    // 着地判定処理
    public void GroundCheck()
    {
        // CheckSphereで着地判定
        isGrounding = Physics.CheckSphere(groundCheckTransform.position,groundCheckDistance,groundLayer);

        // 着地していない場合
        if (!isGrounding)
        {
            /*
            if (transform.parent != null)
            {
                transform.parent = null;
            }
            */
        }
        // 着地している場合
        else
        {
            //ダッシュを可能に
            canDash = true;
        }
    }

    // 攻撃レベルを上げる処理
    public void PlayerAttackLvIncreased()
    {
        // 攻撃レベルを上げる
        playerAttackLv++;
        // 攻撃レベルが4以上の場合
        if (playerAttackLv >= 4)
        {
            playerAttackLv = 4;
        }
        //playerAttackLvCountを現攻撃レベルの最大値カウントに設定
        playerAttackLvCount = playerAttackLv2To4CountMax[Math.Clamp(playerAttackLv-2,0,2) ];
    }

    // 攻撃レベルを下げる処理
    public void PlayerAttackLvDecreased()
    {
        // 攻撃レベルを1下げる
        playerAttackLv--;
        // 攻撃レベルが1以下の場合
        if (playerAttackLv <= 1)
        {
            playerAttackLvCount = 0;
        }
        // それ以外
        else
        {
            //playerAttackLvCountを現攻撃レベルの最大値カウントに設定
            playerAttackLvCount = playerAttackLv2To4CountMax[Math.Clamp(playerAttackLv-2,0,2) ];
        }
    }

    // 攻撃レベル関連の処理
    void PlayerAttackLv()
    {
        //刀モデルのマテリアルを現攻撃レベルのマテリアルに設定
        katanaModel.material = playerAttackLvMaterials[playerAttackLv-1];
        // 攻撃レベルが1より大きい場合
        if (playerAttackLv > 1)
        {
            playerAttackLvCount--;
            // playerAttackLvCountが0以下の場合
            if (playerAttackLvCount <= 0)
            {
                // 攻撃レベルを下げる
                PlayerAttackLvDecreased();
            }
        }

    }
    public void FixedUpdateEvent()
    {
        // 攻撃レベル
        PlayerAttackLv();
    }
    public void UpdateEvent()
    {
        // オブジェクトを一個以上タグしたら、ワープ攻撃が可能
        canWarpAttack = warpAttackManager.taggedObjectList[0].transform;
        // 着地判定
        GroundCheck();
    }
}
