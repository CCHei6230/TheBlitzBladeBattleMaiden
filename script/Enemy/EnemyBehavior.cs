using UnityEngine;
using DG.Tweening;


// iTaggableObject を実装しており、プレイヤーのワープ攻撃のターゲットとなる。
// 各フレームで currentState の処理を実行し、返されたステートへの遷移を管理する。
public class EnemyBehavior : MonoBehaviour, iTaggableObject
{
    // 現在実行中のステート
    StateBase currentState;
    // 次のフレームに遷移するステート
    StateBase nextState;
    // この敵のパラメータ・参照を保持するプロパティクラス
    [SerializeReference] EnemyProperty enemyProperty;

    // プレイヤーのワープ攻撃を受けた際の処理。
    // HP を減算し、0 以下になった場合は死亡処理を実行する。
    public void BeSlashed(PlayerProperty playerProperty = null)
    {
        // ダメージ量：（プレイヤーの攻撃レベル + 1）× 50
        enemyProperty.hp -= 50 * (playerProperty.playerAttackLv + 1);

        if (enemyProperty.hp <= 0)
        {
            Destroy(Instantiate(enemyProperty.deathEffectPrefab,
                transform.position, Quaternion.identity), 2f);
            enemyProperty.modelObject.transform.DOKill();
            // コントローラーを振動させる
            InputHaptic.instance.Haptic(0.35f, 0.0f, 2);
            if (enemyProperty.playerAttack)
            {
                enemyProperty.playerAttack.GetComponentInParent<PlayerWarpAttackManager>().RemoveTagTransform(transform);
            }
            enemyProperty.groupManager.RemoveEnemy(gameObject);
            gameObject.SetActive(false);
        }
    }

    // ワープ攻撃命中後の HP を返す処理。ターゲット選定（致死判定）に使用される。
    // 致死ダメージの場合は事前に Rigidbody を削除して移動を停止する。
    public int HPAfterBeSlashed(int _atkLv)
    {
        if (enemyProperty.hp - 50 * (_atkLv + 1) <= 0)
        {
            Destroy(enemyProperty.RB);
        }
        return enemyProperty.hp - 50 * (_atkLv + 1);
    }

    // この敵のモデルの Transform を返すプロパティ（iTaggableObject インターフェースの実装）
    public Transform ModleTransform { get => enemyProperty.modelObject.transform; }

    private void Awake()
    {
        currentState = enemyProperty.AwakeEvent();
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
