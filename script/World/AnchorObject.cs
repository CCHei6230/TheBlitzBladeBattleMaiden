using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// ワープ攻撃の中継点となるオブジェクト。
// ITaggableObject を継承しており、ワープ攻撃のロックオン対象として機能する。
// TriggerableObject を継承しており、Trigger() で起動・中止が可能。
public class AnchorObject : TriggerableObject ,ITaggableObject
{
    // 表示用の 3D モデルオブジェクト
    [FormerlySerializedAs("modleObject")]
    [SerializeField] GameObject modelObject;
    // オブジェクト生成・出現時に再生するエフェクトのプレハブ
    [FormerlySerializedAs("spwanEffectPrefab")]
    [SerializeField] GameObject spawnEffectPrefab;
    // ワープ攻撃を受けた際に再生するエフェクトのプレハブ
    [SerializeField] GameObject beSlashedEffectPrefab;

    // Trigger() のオーバーライド。オブジェクトの表示・非表示を切り替え、出現エフェクトを再生する。
    public override void Trigger()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        Destroy(Instantiate(spawnEffectPrefab, modelObject.transform.position, Quaternion.identity), 2f);
    }

    // ワープ攻撃を受けた際に実行するコルーチンの参照
    Coroutine Coroutine_BeSlashed = null;

    // ワープ攻撃を受けた際の非同期処理。モデルを一時的に非表示にした後、再表示する。
    IEnumerator IEnumerator_BeSlashed()
    {
        modelObject.SetActive(false);
        // 90 フレーム待機する（約 1.5 秒 / 60FPS 基準）
        for (int i = 0; i < 90; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        modelObject.SetActive(true);
        Destroy(Instantiate(spawnEffectPrefab, modelObject.transform.position, Quaternion.identity), 2f);
        Coroutine_BeSlashed = null;
    }

    // ワープ攻撃を受けた際の処理。エフェクト再生・攻撃レベル上昇・再出現コルーチン起動を行う。
    public void BeSlashed(PlayerProperty playerProperty)
    {
        Destroy(Instantiate(beSlashedEffectPrefab, modelObject.transform.position, Quaternion.identity), 2f);
        // プレイヤーの攻撃レベルを 1 上昇させる
        playerProperty.PlayerAttackLvIncreased();

        // コルーチンが実行中でない場合のみ起動する（二重起動を防ぐ）
        if (Coroutine_BeSlashed == null)
        {
            Coroutine_BeSlashed = StartCoroutine(IEnumerator_BeSlashed());
        }
    }

    // ワープ攻撃後の HP を返す処理。AnchorObject は破壊不可のため、常に int.MaxValue を返す。
    public int HPAfterBeSlashed(int _atkLv)
    {
        return int.MaxValue;
    }

    // モデルの Transform を返すプロパティ（ITaggableObject インターフェースの実装）
    public Transform ModelTransform
    {
        get => modelObject.transform;
    }
}
