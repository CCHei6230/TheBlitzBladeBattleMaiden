using System.Collections;
using UnityEngine;

// 発射物の基底クラス。
// 前方に一定速度で移動し、描画範囲外に出た際は一定時間後に自動削除する機能を持つ。
// EnemyAttack や PhantomBlade などの具体的な攻撃クラスの親クラス。
public class Projectiles : MonoBehaviour
{
    // 速度
    [SerializeField] protected float speed;
    // Rigidbody コンポーネント（物理移動に使用）
    [SerializeField] protected Rigidbody RB;
    // Renderer コンポーネント（描画範囲の判定に使用）
    [SerializeField] protected Renderer renderer;
    // オブジェクト削除時に再生するエフェクトのプレハブ
    [SerializeField] protected GameObject destroyEffect;

    // 描画範囲外での自動削除コルーチンの参照
    protected Coroutine Coroutine_WaitToDestroyWhileNotVisible = null;

    // 描画範囲外に出た際に一定時間後にオブジェクトを削除する非同期処理。
    // _seconds : 削除までの待機時間
    protected IEnumerator IEnumerator_WaitToDestroyWhileNotVisible(int _seconds)
    {
        // 指定秒数待機する
        yield return new WaitForSecondsRealtime(_seconds);
        Destroy(gameObject);
    }

    public void Awake()
    {
        RB = GetComponent<Rigidbody>();
        if (!renderer)
        {
            renderer = GetComponentsInChildren<Renderer>()[0];
        }
        Coroutine_WaitToDestroyWhileNotVisible = null;
    }

    public void Update()
    {
        if (!renderer.isVisible)
        {
            if (Coroutine_WaitToDestroyWhileNotVisible == null)
            {
                Coroutine_WaitToDestroyWhileNotVisible = StartCoroutine(IEnumerator_WaitToDestroyWhileNotVisible(4));
            }
        }
        else
        {
            if (Coroutine_WaitToDestroyWhileNotVisible != null)
            {
                StopCoroutine(Coroutine_WaitToDestroyWhileNotVisible);
                Coroutine_WaitToDestroyWhileNotVisible = null;
            }
        }
    }

    public void FixedUpdate()
    {
        if (RB)
            RB.linearVelocity = transform.forward * speed * Time.fixedDeltaTime;
    }
}
