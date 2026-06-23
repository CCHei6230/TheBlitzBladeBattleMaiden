using UnityEngine;
using System.Collections;
using DG.Tweening;

// 残像エフェクトを生成するユーティリティクラス。
// MeshRenderer と SkinnedMeshRenderer の両方に対応した静的コルーチン


// 参考元: https://www.youtube.com/watch?v=7vvycc2iX6E&t=444s (CHARACTER TRAIL TUTORIAL in Unity)
public class MeshTrail : MonoBehaviour
{
    // MeshRenderer を持つオブジェクト向けの残像生成コルーチン。
    // _objectPool      : 残像オブジェクトを管理するオブジェクトプール
    // _ToMakeTrail     : 残像を生成する対象オブジェクト
    // _activeFrame     : 残像を生成し続けるフレーム数
    // _meshRefreshTime : 何フレームごとに残像を生成するか
    // _destroyTime     : 残像が消えるまでのフレーム数
    // _material        : 残像に使用するマテリアル
    // _materialProperty: フェードアウトに使用するマテリアルプロパティ名
    public static IEnumerator IEnumerator_Trail(ObjectPool _objectPool, GameObject _ToMakeTrail, int _activeFrame,
                                                int _meshRefreshTime, int _destroyTime,
                                                Material _material, string _materialProperty = null)
    {
        yield return new WaitForFixedUpdate();
        MeshFilter[] meshFilters = _ToMakeTrail.GetComponentsInChildren<MeshFilter>();
        // MeshFilter が存在しない場合は処理を終了する
        if (meshFilters.Length == 0)
        {
            yield break;
        }

        for (int i = _activeFrame; i > 0; i--)
        {
            // _meshRefreshTime フレームごとに残像を生成する
            if (i % _meshRefreshTime == 0)
            {
                Material tmp_mat = new Material(_material);
                if (_materialProperty != null)
                {
                    tmp_mat.DOColor(new Color(0, 0, 0, 0), _materialProperty, _destroyTime / 60f);
                }
                else
                {
                    tmp_mat.DOFade(0, _destroyTime / 60f);
                }
                for (int j = 0; j < meshFilters.Length; j++)
                {
                    GameObject meshObj = _objectPool.GetObject();
                    meshObj.transform.SetPositionAndRotation(meshFilters[j].transform.position, meshFilters[j].transform.rotation);
                    meshObj.transform.localScale = meshFilters[j].transform.lossyScale;
                    meshObj.GetComponent<MeshRenderer>().material = tmp_mat;
                    meshObj.GetComponent<MeshFilter>().mesh = meshFilters[j].mesh;
                    // 指定フレーム後にオブジェクトをプールに戻す
                    _objectPool.ReturnObject(meshObj, _destroyTime);
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    // SkinnedMeshRenderer を持つキャラクター向けの残像生成コルーチン。
    // アニメーション中のポーズをベイクして残像を生成するため、スケルタルアニメーションに対応している。
    // 引数の意味は IEnumerator_Trail と同様。
    public static IEnumerator IEnumerator_TrailSkinnedMesh(ObjectPool _objectPool, GameObject _ToMakeTrail, int _activeFrame,
        int _meshRefreshTime, int _destroyTime,
        Material _material, string _materialProperty = null)
    {
        yield return new WaitForFixedUpdate();
        SkinnedMeshRenderer[] skinnedMeshRenderers = _ToMakeTrail.GetComponentsInChildren<SkinnedMeshRenderer>();
        // SkinnedMeshRenderer が存在しない場合は処理を終了する
        if (skinnedMeshRenderers.Length == 0)
        {
            yield break;
        }

        for (int i = _activeFrame; i > 0; i--)
        {
            // _meshRefreshTime フレームごとに残像を生成する
            if (i % _meshRefreshTime == 0)
            {
                Material tmp_mat = new Material(_material);
                if (_materialProperty != null)
                {
                    tmp_mat.DOColor(new Color(0, 0, 0, 0), _materialProperty, _destroyTime / 60f);
                }
                else
                {
                    tmp_mat.DOFade(0, _destroyTime / 60f);
                }
                for (int j = 0; j < skinnedMeshRenderers.Length; j++)
                {
                    GameObject meshObj = _objectPool.GetObject();
                    meshObj.transform.SetPositionAndRotation(skinnedMeshRenderers[j].transform.position, skinnedMeshRenderers[j].transform.rotation);
                    // 現在のアニメーションポーズのメッシュをベイクして残像に使用する
                    skinnedMeshRenderers[j].BakeMesh(meshObj.GetComponent<MeshFilter>().mesh);
                    meshObj.GetComponent<MeshRenderer>().material = tmp_mat;
                    // 指定フレーム後にオブジェクトをプールに戻す
                    _objectPool.ReturnObject(meshObj, _destroyTime);
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
