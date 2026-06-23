using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// ヒットストップ（時間を一時的に遅くする演出）を管理するシングルトンクラス。
// 時間スケールの変更とカメラの画面振動を組み合わせてインパクト感のある演出を実現する。
public class HitStopManager : MonoBehaviour
{
    public static HitStopManager instance = null;
    // 画面振動を発生させる Cinemachine のインパルスソース
    [SerializeReference] CinemachineImpulseSource impulseSource = null;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
    }

    // ヒットストップを実行する処理。
    // _timeScale : ヒットストップ中のゲーム速度（0 に近いほどスローになる）
    // _countMax  : ヒットストップの持続フレーム数（60FPS 基準）
    // _override  : true の場合、実行中のヒットストップを中断して新しい処理を開始する
    public void HitStop(float _timeScale, int _countMax, bool _override = false)
    {
        // _override が true かつ既にヒットストップが実行中の場合は中断する
        if (_override && Coroutine_HitStop != null)
        {
            StopCoroutine(Coroutine_HitStop);
            Coroutine_HitStop = null;
        }
        if (Coroutine_HitStop == null)
        {
            Coroutine_HitStop = StartCoroutine(IEnumerator_HitStop(_timeScale, _countMax));
        }
    }

    // 実行中のヒットストップコルーチンの参照
    public Coroutine Coroutine_HitStop = null;

    // ヒットストップの非同期処理本体。時間スケールを変更し、指定フレーム数後に元に戻す。
    IEnumerator IEnumerator_HitStop(float _timeScale, int _countMax)
    {
        Time.timeScale = _timeScale;
        // 画面振動を発生させる
        impulseSource.GenerateImpulse();

        // 指定フレーム数待機する
        int tmp_count = 0;
        while (tmp_count < _countMax)
        {
            tmp_count++;
            yield return new WaitForFixedUpdate();
        }
        // ゲームの時間スケールを通常（1）に戻す
        Time.timeScale = 1;
        Coroutine_HitStop = null;
    }
}
