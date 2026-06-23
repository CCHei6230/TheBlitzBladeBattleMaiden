using System;
using DG.Tweening;
using UnityEngine;


// iTaggableObject を実装しており、プレイヤーのワープ攻撃の対象となる。
// TriggerableObject を継承しており、外部から Trigger() で起動できる。
public class BossBehavior : TriggerableObject, iTaggableObject
{
    // ボスのパラメータ・参照を保持するプロパティクラス
    BossProperty _bossProperty;
    // 現在実行中のステート
    StateBase currentState;
    // 次のフレームに遷移するステート
    StateBase nextState;

    // プレイヤーのワープ攻撃を受けた際の処理。バリアがない場合にダメージを与え、HP が 0 以下なら死亡処理を実行する。
    public void BeSlashed(PlayerProperty playerProperty)
    {
        if (!_bossProperty.barrierObject)
        {
            // ダメージ量：（プレイヤーの攻撃レベル + 1）× 50
            _bossProperty.hp -= 50 * (playerProperty.playerAttackLv + 1);
            _bossProperty.BossUIImage.DoFillAmount(_bossProperty.hp, _bossProperty.HPMax, 0.15f);
            // HP バーを赤くフラッシュさせる
            _bossProperty.BossUIImage.DoColorToAndBack(Color.red, 0.2f);
            if (_bossProperty.hp <= 0)
            {
                _bossProperty.Death();
            }
        }
    }

    // ワープ攻撃が命中した場合の、ダメージ適用後の HP を返す処理。
    // ワープ攻撃のターゲット選定（致死判定）に使用される。
    public int HPAfterBeSlashed(int _atkLv)
    {
        if (_bossProperty.barrierBreakCount < 0  )
        {
            return int.MaxValue;
        }
        return _bossProperty.hp - 50 * (_atkLv + 1);
    }

    // ボスのモデルの Transform を返すプロパティ（iTaggableObject インターフェースの実装）
    public Transform ModleTransform { get => _bossProperty.ModleTransform; }

    private void Awake()
    {
        _bossProperty = gameObject.GetComponent<BossProperty>();
        currentState = _bossProperty.AwakeEvent();
        currentState.StateEnterEvent();
        nextState = currentState;
    }

    void Update()
    {
        nextState = currentState.UpdateEvent();
        // nextState が currentState と異なる場合はステートを切り替える
        if (nextState != currentState)
        {
            ChangeState(nextState);
        }
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateEvent();
    }

    void OnTriggerEnter(Collider other)
    {
        currentState.TriggerEnterEvent(other);
    }

    void OnTriggerStay(Collider other)
    {
        currentState.TriggerStayEvent(other);
    }

    void OnTriggerExit(Collider other)
    {
        currentState.TriggerExitEvent(other);
    }

    // ステートを切り替える処理。現在のステートの退出処理を行い、新しいステートの入場処理を実行する。
    void ChangeState(StateBase _state)
    {
        currentState.ExitEvent();
        // 新しいステートをセットする
        currentState = _state;
        currentState.StateEnterEvent();
    }
}
