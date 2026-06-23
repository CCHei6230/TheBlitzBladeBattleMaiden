using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


// PlayerProperty・LockOnManager・WarpAttackManager の各 Update/FixedUpdate 処理を統括し、
// currentState の処理を実行して状態遷移を管理する。
public class PlayerBehaviour : MonoBehaviour
{
    // 現在実行中のステート
    StateBase currentState;
    // 次のフレームに遷移するステート
    StateBase nextState;
    // プレイヤーのパラメータ・参照を保持するプロパティクラス
    [SerializeField] PlayerProperty playerProperty;
    // ロックオン機能を管理するマネージャー
    [SerializeField] PlayerObjectLockOnManager objectLockOnManager;
    // ワープ攻撃機能を管理するマネージャー
    [SerializeField] PlayerWarpAttackManager warpAttackManager;

    void Awake()
    {
        AudioClipManager.instance.LoopAudio("BGM1");
        
        // CheckPointManager からプレイヤーの生成位置と向きを取得して適用する
        var tmp_checkPoint = CheckPointManager.instance;
        if (tmp_checkPoint)
        {
            playerProperty.RB.position = tmp_checkPoint.spawnPosition;
            playerProperty.modelObject.transform.rotation = Quaternion.Euler(tmp_checkPoint.spawnRotation);
            playerProperty.lastForward = playerProperty.modelObject.transform.forward;
        }
        
        // 画面フェードイン演出を開始する
        var tmp_uiFade = UI_FadeScreen.instance;
        if (tmp_uiFade)
        {
            tmp_uiFade.Fade(true);
        }

        // 各マネージャーとプロパティの初期化を行い、初期ステートを設定する
        playerProperty.AwakeEvent();
        objectLockOnManager.playerProperty = playerProperty;
        warpAttackManager.playerProperty = playerProperty;
        warpAttackManager.AwakeEvent();
        currentState = playerProperty.stateDictionary[PlayerState.idle];
        currentState.StateEnterEvent();
        nextState = currentState;
    }

    void Update()
    {
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            playerProperty.HP = 1;
        }
        // 各マネージャーの Update 処理を実行する
        objectLockOnManager.UpdateEvent();
        playerProperty.UpdateEvent();
        warpAttackManager.UpdateEvent();
        nextState = currentState.UpdateEvent();
        // nextState が currentState と異なる場合はステートを切り替える
        if (nextState != currentState)
        {
            ChangeState(nextState);
        }
    }

    void FixedUpdate()
    {
        // 各マネージャーの FixedUpdate 処理を実行する
        playerProperty.FixedUpdateEvent();
        warpAttackManager.FixedUpdateEvent();
        
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
        currentState = _state;
        currentState.StateEnterEvent();
    }
}
