using DG.Tweening;
using UnityEngine;

// 結界オブジェクト。
// TriggerableObject を継承しており、Trigger() で結界の展開・消滅を切り替える。
// マテリアルプロパティ「_step」を DOTween でアニメーションさせることで、結界の出現・消滅演出を行う。
public class BarrierObject : TriggerableObject
{
    // 結界の出現・消滅演出に使用する Renderer の配列
    [SerializeField] Renderer[] renderers;

    // Trigger() のオーバーライド。現在の Active 状態に応じて展開または消滅処理を実行する。
    public override void Trigger()
    {
        // 結界が展開中（Active）の場合は消滅処理を行う
        if (gameObject.activeSelf)
        {
            GetComponent<Collider>().enabled = false;

            // renderers[1] 以降の _step を即座に 1（全表示）にする
            // その後 1.5 秒かけて 0（透明）にアニメーションさせ、結界を溶かして消す
            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].material.DOFloat(1, "_step", 0);
            }

            renderers[0].material.DOFloat(0, "_step", 1.5f)
                .OnComplete(() => { Destroy(gameObject); });

            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].material.DOFloat(0, "_step", 1.5f);
            }
        }
        // 結界が非表示（非 Active）の場合は展開処理を行う
        if (!gameObject.activeSelf)
        {
            
            GetComponent<Collider>().enabled = true;
            // オブジェクトを表示状態にする
            gameObject.SetActive(true);
            // 全 Renderer の _step を即座に 0（非表示）にする
            // 1.5 秒かけて _step を 1（全表示）にアニメーションさせ、結界を徐々に形成する
            for (int i = 1; i < renderers.Length; i++)
            {
                renderers[i].material.DOFloat(0, "_step", 0);
            }
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.DOFloat(1, "_step", 1.5f);
            }
        }
    }
}
