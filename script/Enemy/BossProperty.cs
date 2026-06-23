using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

// ボスのパラメータ・参照・ステート管理するクラス。
// EnemyPropertyBase を継承しており、基本的な敵の属性を引き継ぐ。
public class BossProperty : EnemyPropertyBase
{
    // 召喚する敵グループのプレハブ配列
    [FormerlySerializedAs("EnemyGruopPrefab")]
    public GameObject[] EnemyGroupPrefab;
    // ボスが使用する攻撃オブジェクトのプレハブ配列
    public GameObject[] AttackPrefab;
    // 現在アクティブな敵グループのマネージャー
    public EnemyGroupManager currentEnemyGroup;
    // バリアオブジェクトのプレハブ
    public GameObject barrierPrefab;
    // 現在展開中のバリアオブジェクト（null の場合はバリアなし）
    public TriggerableObject barrierObject;
    // テレポート先として使用できる Transform の配列
    public Transform[] teleportPositions;
    // 前回テレポートした位置のインデックス（同じ場所への連続テレポートを防ぐために使用）
    public int lastPositionIndex = 0;
    // HP の最大値
    public int HPMax = 20000;
    // ボスの HP を表示する UI オブジェクト
    public UI_ImageFillObject BossUIImage;
    // バリア破壊後の硬直時間（フレーム数）。-1 で硬直中を示す。
    public int barrierBreakDuration = -1;
    // バリアを破壊された累計回数
    public int barrierBreakCount = 0;
    // 硬直カウントの上限値
    public int barrierBreakCountMax = 15;
    // 残像エフェクトに使用するマテリアル
    public Material trailMaterial;
    // 残像エフェクト用オブジェクトのオブジェクトプール
    public ObjectPool trailObjectPool;
    // ボスのステートを管理する Dictionary（BossState 列挙型をキーとする）
    public Dictionary<BossState, BossStateBase> stateDictionary { get; private set; }

    // 初期化処理。全ステートを生成して Dictionary に登録し、初期ステートを返す。
    public override StateBase AwakeEvent()
    {
        RB = GetComponent<Rigidbody>();
        stateDictionary = new Dictionary<BossState, BossStateBase>()
        {
            { BossState.Standby,    new BossState_StandbyState()   },
            { BossState.Shielding,  new BossState_ShieldingState() },
            { BossState.WaitToMove, new BossState_WaitToMoveState()},
            { BossState.Moving,     new BossState_MovingState()    },
        };
        stateDictionary[BossState.Standby].SetProperty(this);
        hp = HPMax;
        // 初期ステートとして Standby を返す
        return stateDictionary[BossState.Standby];
    }

    // ダメージ処理のオーバーライド。バリアが存在しない場合のみダメージを適用し、HP が 0 以下なら死亡処理を実行する。
    public override void DealDamage(PlayerAttack _playerAttack)
    {
        if (!barrierObject)
        {
            base.DealDamage(_playerAttack);
            BossUIImage.DoFillAmount(hp, HPMax, 0.15f);
            // HP バーを赤くフラッシュさせる
            BossUIImage.DoColorToAndBack(Color.red, 0.2f);
            if (hp <= 0)
            {
                Death();
            }
        }
    }

    // ボスの死亡処理。エフェクト再生・非表示化・コントローラー振動・タイトル遷移を行う。
    public void Death()
    {
        var tmp_eff = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        tmp_eff.transform.localScale = Vector3.one * 3f;
        Destroy(tmp_eff, 2f);
        modelObject.transform.DOKill();
        gameObject.SetActive(false);
        // コントローラーを振動させる
        InputHaptic.instance.Haptic(0.35f, 0.0f, 2);
        Invoke("Invoke_Death", 0.75f);
    }

    // 死亡後のシーン遷移処理。フェードアウト後にタイトルシーンへ移行する。
    void Invoke_Death()
    {
        var tmp_uiFade = UI_FadeScreen.instance;
        if (tmp_uiFade)
        {
            CheckPointManager.instance.EndGame();
            tmp_uiFade.Fade(false, InGameSceneManager.instance.TitleScene);
        }
    }
}
