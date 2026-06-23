using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

// コントローラーの振動（ハプティクス）を管理するシングルトンクラス。
// 振動の強度と持続フレーム数を指定して再生できる。振動中に再度呼び出した場合は前の振動を中断して新しい振動を開始する。
public class InputHaptic : MonoBehaviour
{
    // シングルトンのインスタンス参照
    public static InputHaptic instance;
    private PlayerInput playerInput;
    // 振動をサポートするゲームパッドのインターフェース参照
    private IDualMotorRumble gamepad;
    // 実行中の振動コルーチンの参照
    Coroutine Coroutine_Haptic = null;

    // 振動の非同期処理。指定フレーム数振動させた後に停止する。
    IEnumerator IEnumerator_Haptic(float _lowFrequency, float _highFrequency, int _duration)
    {
        // 低周波・高周波モーターの振動強度を設定する
        gamepad.SetMotorSpeeds(_lowFrequency, _highFrequency);
        // 指定フレーム数待機する
        for (int i = 0; i < _duration; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        // 振動を停止
        gamepad.SetMotorSpeeds(0, 0);
        Coroutine_Haptic = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // 振動を再生する処理。
    // _lowFrequency  : 低周波モーターの強度
    // _highFrequency : 高周波モーターの強度
    // _duration      : 振動の持続フレーム数（60FPS 基準）
    public void Haptic(float _lowFrequency, float _highFrequency, int _duration)
    {
        // 接続中のデバイスから IDualMotorRumble を実装するゲームパッドを探す
        foreach (var device in playerInput.devices)
        {
            if (device is IDualMotorRumble rumbleDevice)
            {
                gamepad = rumbleDevice;
                break;
            }
        }
        // 対応するゲームパッドが見つからない場合は処理を中断する
        if (gamepad == null)
        {
            return;
        }
        // 既に振動中の場合は現在の振動を中断して停止する
        if (Coroutine_Haptic != null)
        {
            StopCoroutine(Coroutine_Haptic);
            gamepad.SetMotorSpeeds(0, 0);
        }
        // 振動コルーチンを開始
        Coroutine_Haptic = StartCoroutine(IEnumerator_Haptic(_lowFrequency, _highFrequency, _duration));
    }
}
