using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class PlayerWarpAttackManager : MonoBehaviour
{
    // タグオブジェクトの定義
    [Serializable]
    public class TaggedObject
    {
        // オブジェクトのtransform
        public Transform transform;
        // オブジェクトのカウント
        public int taggedCount ;
        // タグした時刻
        public float timeBeTagged;
        // オブジェクトのRenderer
        public Renderer renderer;
    }
    // タグリスト
    public List<TaggedObject>  taggedObjectList ;
    // タグカウントの最大値
    public int taggedCountMax = 600;
    // タグしたオブジェクトの順番を示すlineRenderer
    public LineRenderer lineRenderer;
    // ワープ攻撃のエフェクトとして使うlineRenderer
    public LineRenderer lineRenderer2;

    // 集中モード中に探知してタグしたオブジェクトのリスト
    public List<Transform> scopeTaggedList;
    // 集中モードのUI
    [SerializeField] private Image scope;

    // 幻影剣のエフェクト
    [SerializeField]GameObject phantomBladeEffect;
    // 幻影剣のプレハブ
    [SerializeField]GameObject phantomBladeShotPrefab;
    // 幻影剣の発射位置
    [SerializeField]Transform ShotPosition;

    public PlayerProperty playerProperty;

    public void AwakeEvent()
    {
        //タグリストの初期化
        taggedObjectList  =new List<TaggedObject>(){new TaggedObject(),new TaggedObject(),new TaggedObject(),new TaggedObject(),new TaggedObject()};
        for (int i = 0; i < taggedObjectList.Count; i++)
        {
            taggedObjectList[i].taggedCount =-1;
            taggedObjectList[i].timeBeTagged =  float.PositiveInfinity;
        }
    }
    public void UpdateEvent()
    {

        // このフレームに集中ボタンを離し、かつ入力受け可能な場合
        if (playerProperty.playerInput.ScopeInput.WasReleasedThisFrame() && playerProperty.playerInput.canInput)
        {
            Time.timeScale = 1;
            // 集中タグリストを解放
            scopeTaggedList.Clear();
            //集中UIを非表示
            scope.DOFade(0, 0.2f);
        }

        // 集中ボタンを押している、
        // かつワープ攻撃していない、
        // かつfocusCountが0以上、
        // かつ強制リロードしていない、
        // かつ入力受け可能な場合
        if (playerProperty.playerInput.ScopeInput.IsPressed() && !playerProperty.warpAttacking
            && playerProperty.focusCount > 0 && playerProperty.Coroutine_FocusReload == null && playerProperty.playerInput.canInput)
        {
            Time.timeScale = 0.4f;
            //集中UIを表示
            scope.DOFade(1, 0.2f);

            // カメラの位置と向きでRaycastし、タグ可能なオブジェクトを探知する
            RaycastHit hit;
            Vector3 origin = playerProperty.cameraTransform.position;
            Vector3 direction = playerProperty.cameraTransform.forward;
            if (Physics.Raycast(origin, direction,out hit,150))
            {
                ITaggableObject tmp_tagObj = hit.transform.GetComponentInParent<ITaggableObject>();

                // タグ可能なオブジェクトを探知した場合
                if(tmp_tagObj != null)
                {
                    // そのオブジェクトが既にscopeTaggedListにあるかをチェック(探知過程に既に探知しタグされたか)
                    bool tmp_hadBeenLockoned = false;
                    foreach (var lockonedTarget in scopeTaggedList)
                    {
                        if (lockonedTarget == tmp_tagObj.ModleTransform)
                        {
                            tmp_hadBeenLockoned = true;
                        }
                    }

                    // まだ探知されたことがない場合
                    if (!tmp_hadBeenLockoned)
                    {
                        // scopeTaggedListの要素数がtaggedObjectListより小さい場合
                        if (scopeTaggedList.Count < taggedObjectList.Count)
                        {
                            // オブジェクトをタグ
                            TagObject(tmp_tagObj.ModleTransform);
                            //scopeTaggedListに追加(探知過程に既に探知しタグされた)
                            scopeTaggedList.Add(tmp_tagObj.ModleTransform);
                            // コントローラーを振動させる
                            InputHaptic.instance.Haptic(0,0.01f,2);
                        }
                    }

                }
            }

        }
        // それ以外
        else
        {
            // ヒットストップしていない場合
            if (HitStopManager.instance.Coroutine_HitStop == null)
            {
                Time.timeScale = 1;
            }


            // 発射ボタンを押した、かつワープ攻撃していない、かつ入力受け可能な場合
            if(playerProperty.playerInput.shotInput.WasPressedThisFrame()
               && !playerProperty.warpAttacking
               && playerProperty.playerInput.canInput )
            {
                ShotPosition.localRotation =Quaternion.identity;

                // ロックオンしている場合
                if (playerProperty.lockOnEnemy != null)
                {
                    // ターゲットに向かって幻影剣を発射
                    var tmp_shikigami = Instantiate(phantomBladeShotPrefab, ShotPosition.position, Quaternion.identity);
                    tmp_shikigami.GetComponent<PhantomBlade>().SetData(5000, this,ShotPosition.position, playerProperty.lockOnEnemy);

                }

                // ロックオンしていない場合
                else
                {
                    // プレイヤーのモデルの向きに幻影剣を発射
                    var tmp_shikigami = Instantiate(phantomBladeShotPrefab, ShotPosition.position, playerProperty.modelObject.transform.rotation);
                    tmp_shikigami.GetComponent<PhantomBlade>().SetData(5000, this,ShotPosition.position);

                }
                // エフェクトを生成し、1秒後に消す
                Destroy(
                    Instantiate(phantomBladeEffect, ShotPosition.position, Quaternion.identity)
                    ,1f);
                // 発射SFXを再生
                AudioClipManager.instance.PlayAudioOneShot("SFX_Shoot");
            }
        }

        // プレイヤーのモデルの位置でlineRendererの起点位置を更新
        lineRenderer.positionCount  = 1;
        if (playerProperty.modelObject)
        {
            lineRenderer.SetPosition( 0,playerProperty.modelObject.transform.position);
        }
        
        // タグリストの要素でlineRendererの通過点の数と位置を更新
        for (int i = 0; i < taggedObjectList.Count; i++)
        {
            // 要素が存在している場合
            if (taggedObjectList[i].transform)
            {     
                // lineRendererの通過点を増やし、位置を設定
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount-1, taggedObjectList[i].transform.position);
            }
        }
    }

    // 強制リロード処理
    IEnumerator IEnumerator_FocusReload()
    {
        while (playerProperty.focusCount <playerProperty.focusCountMax )
        {
            playerProperty.focusCount ++;
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        playerProperty.Coroutine_FocusReload = null;
    }
    // 回復処理
    IEnumerator IEnumerator_FocusFilling()
    {
        while (playerProperty.focusCount <playerProperty.focusCountMax )
        {
            playerProperty.focusCount ++;
            for (int i = 0; i < 2; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        playerProperty.Coroutine_FocusFilling = null;
    }
    public void FixedUpdateEvent()
    {
        // focusCountがfocusCountMaxより大きい場合
        if (playerProperty.focusCount >= playerProperty.focusCountMax)
        {
            // focusCountの最大値を制限する
            playerProperty.focusCount = playerProperty.focusCountMax;
        }

        // 集中ボタンを押している、
        // かつ入力受け可能、
        // かつワープ攻撃してない、
        // かつfocusCountが強制リロードしていない場合
        if (playerProperty.playerInput.ScopeInput.IsPressed()&& playerProperty.playerInput.canInput && !playerProperty.warpAttacking
            && playerProperty.focusCount > 0 && playerProperty.Coroutine_FocusReload == null)
        {
            playerProperty.focusCount--;
            // focusCountが０以下になった場合
            if (playerProperty.focusCount <= 0)
            {
                // 強制リロード処理を行う
                playerProperty.Coroutine_FocusReload = StartCoroutine(IEnumerator_FocusReload());
            }
            // focusCountの回復処理が行っている場合
            if (playerProperty.Coroutine_FocusFilling != null)
            {
                // 回復処理を中止
                StopCoroutine(playerProperty.Coroutine_FocusFilling );
                playerProperty.Coroutine_FocusFilling =null;
            }
        }

        // または、
        // 集中ボタンを押していない、
        // かつfocusCountが強制リロードしていない、
        // かつfocusCountが回復処理していない、
        // かつfocusCountが最大値より小さい場合
        else if(!playerProperty.playerInput.ScopeInput.IsPressed()  && playerProperty.Coroutine_FocusReload == null
                && playerProperty.Coroutine_FocusFilling == null && playerProperty.focusCount  < playerProperty.focusCountMax )
        {
            // 回復処理を行う
            playerProperty.Coroutine_FocusFilling = StartCoroutine(IEnumerator_FocusFilling());
        }


        // タグリストを常にチェック
        for (int i = 0; i < taggedObjectList.Count; i++)
        {
            // 要素のtransformが存在してない場合
            if(!taggedObjectList[i].transform)
            {
                // 要素のカウントが-1より大きい場合(つまりタグ中に対象が消滅した場合)
                if (taggedObjectList[i].taggedCount > -1)
                {
                    // 要素を初期化する(タグ解除)
                    taggedObjectList[i].taggedCount =-1;
                    taggedObjectList[i].transform = null;
                    taggedObjectList[i].renderer = null;
                    taggedObjectList[i].timeBeTagged = float.PositiveInfinity;
                    // タグした時刻でタグリストをソートする
                    taggedObjectList = taggedObjectList.OrderBy(x => x.timeBeTagged).ToList();
                    // lineRendererを初期化する
                    lineRenderer.positionCount = 0;
                }
                // 次の要素をチェック
                continue;
            }
            // タグしているオブジェクトのカウントが-1より大きい場合
            if (taggedObjectList[i].taggedCount > -1)
            {
                // カウント減らし続ける
                taggedObjectList[i].taggedCount--;
            }
            // タグしているオブジェクトのカウントが-1以下の場合
            if (taggedObjectList[i].taggedCount <= -1 )
            {
                // 要素を初期化する(タグ解除)
                taggedObjectList[i].transform = null;
                taggedObjectList[i].renderer = null;
                taggedObjectList[i].timeBeTagged = float.PositiveInfinity;
                // タグした時刻でタグリストをソートする
                taggedObjectList = taggedObjectList.OrderBy(x => x.timeBeTagged).ToList();
                // lineRendererを初期化する
                lineRenderer.positionCount = 0;

            }
        }
    }
    // オブジェクトをタグし、リストに収納する処理
    public void TagObject(Transform _objectTransform)
    {
        // ワープ攻撃中はタグできない
        if (playerProperty.warpAttacking)
        {
            // 処理中止
            return;
        }
        // タグSFXを再生
        AudioClipManager.instance.PlayAudioOneShot("SFX_Tagged");

        // タグしたオブジェクトのインデックスを決める為の変数
        int tmp_index = 0;
        for (int i = 0; i < taggedObjectList.Count; i++)
        {
            // 要素iのtransformがnull(タグしていない)場合
            if (!taggedObjectList[i].transform )
            {
                tmp_index = i;
                // forを中止
                break;
            }
            // タグしたオブジェクトが既にリストに存在している場合
            if (taggedObjectList[i].transform == _objectTransform)
            {
                // タグのカウントを最大値に
                taggedObjectList[i].taggedCount = taggedCountMax;
                // 処理中止
                return;
            }
        }
        // タグしたオブジェクトのtransformを記録
        taggedObjectList[tmp_index].transform  = _objectTransform;
        // タグした時刻を記録
        taggedObjectList[tmp_index].timeBeTagged = Time.time;
        taggedObjectList[tmp_index].taggedCount =taggedCountMax;
        taggedObjectList[tmp_index].renderer = _objectTransform.GetComponentInChildren<Renderer>();
        // タグリストが2体以上のオブジェクトが存在した場合
        if (taggedObjectList.Count > 2)
        {
            // タグした時刻でタグリストをソートする
            taggedObjectList = taggedObjectList.OrderBy(x => x.timeBeTagged).ToList();
        }
        lineRenderer.positionCount = 0;
    }

    // Transformで指定したオブジェクトをタグリストから解除する処理
    public void RemoveTagTransform(Transform _transformToRemove)
    {
        for (int i = 0; i < taggedObjectList.Count; i++)
        {
            // 要素iのtransformが_transformToRemoveと同じ場合
            if (taggedObjectList[i].transform != _transformToRemove)
            {
                // 次の要素をチェック
                continue;
            }
            // 要素を初期化する(タグ解除)
            taggedObjectList[i].transform = null;
            taggedObjectList[i].renderer = null;
            taggedObjectList[i].taggedCount = -1;
            taggedObjectList[i].timeBeTagged = float.PositiveInfinity;
            lineRenderer.positionCount = 0;
        }
    }

    // タグしたオブジェクトを解除する処理
    public void ClearTagObject()
    {
        for (int i = 0; i < taggedObjectList.Count; i++)
        {
            // 要素を初期化する(タグ解除)
            taggedObjectList[i].transform = null;
            taggedObjectList[i].renderer = null;
            taggedObjectList[i].taggedCount = -1;
            taggedObjectList[i].timeBeTagged = float.PositiveInfinity;
            lineRenderer.positionCount = 0;
        }
    }
}
