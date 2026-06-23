using System.Collections;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

// EnemyAttack の派生クラス。レーザー型の攻撃オブジェクト。
// スケールを拡大・縮小させることでレーザーの射出演出を行い、スケールが一定値を超えた際に当たり判定を有効にする。
public class EnemyAttackLaser : EnemyAttack
{
    // レーザーのビジュアルモデル
    [SerializeField] GameObject model;
    // レーザーの現在のスケール値
    [SerializeField] float currentScale;
    // レーザーが最大まで拡大した際の目標スケール値
    [SerializeField] float targetScale;
    // レーザーの持続フレーム数（前半で拡大、後半で縮小）
    [SerializeField] int duration;
    // レーザーの当たり判定コライダー
    [SerializeField] Collider collider;

    // レーザーのスケールを時間経過に応じて拡大・縮小させるコルーチン。
    // 前半で拡大しながら当たり判定を有効化、後半で縮小しながら当たり判定を無効化する。
    IEnumerator IEnumerator_Scaling()
    {
        // 1 フレームあたりのスケール変化量（目標スケール ÷ 全持続フレーム数）
        var tmp_ratio = targetScale / (float)duration;
        currentScale = 0;

        // 前半：スケールを拡大していく
        for (int i = 0; i < duration / 2; i++)
        {
            currentScale += tmp_ratio;
            yield return new WaitForFixedUpdate();

            if (currentScale > 0.3f && !collider.enabled)
            {
                collider.enabled = true;
            }
        }

        // 後半：スケールを縮小していく
        for (int i = 0; i < duration / 2; i++)
        {
            currentScale -= tmp_ratio;
            yield return new WaitForFixedUpdate();

            if (currentScale < 0.3f && collider.enabled)
            {
                collider.enabled = false;
            }
        }
        Destroy(gameObject);
    }

    protected void Start()
    {
        base.Start();
        collider = GetComponentInChildren<Collider>();
        StartCoroutine(IEnumerator_Scaling());
    }

    private void Update()
    {
        model.transform.localScale = new Vector3(currentScale, 1, currentScale);
    }

    // パリィされた際の処理。何もしない。
    public override void BeParry(PlayerProperty playerProperty) { return; }

    // 地面との接触処理。レーザーは地面に当たっても消えないため何もしない。
    protected override void OnTriggerEnter(Collider other) { return; }
}
