using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 汎用オブジェクトプールクラス。
// 参考元: https://www.youtube.com/watch?v=3EGXrJRCWLg
// 頻繁に生成・削除されるオブジェクトを事前にプールとして保持することで、
// Instantiate/Destroy のコストを削減しパフォーマンスを向上させる。
public class ObjectPool : MonoBehaviour
{
    // プールで管理するオブジェクトのプレハブ
    public GameObject prefab;
    // プールの初期容量（Awake 時にこの数だけ生成される）
    [SerializeField] int capacity;
    // 非アクティブなオブジェクトを管理する Queue
    private Queue<GameObject> pool;

    private void Awake()
    {
        pool = new Queue<GameObject>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            GameObject tmp_obj = Instantiate(prefab, transform);
            tmp_obj.SetActive(false);
            pool.Enqueue(tmp_obj);
        }
    }

    // プールからオブジェクトを取り出してアクティブ化して返す。
    // プールが空の場合は新しくオブジェクトを生成して返す。
    public GameObject GetObject()
    {
        GameObject obj = null;
        // プールにオブジェクトが残っている場合はプールから取り出す
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
            obj.transform.SetParent(transform);
            return obj;
        }
        obj = Instantiate(prefab);
        obj.transform.SetParent(transform);
        return obj;
    }

    // 指定したフレーム数後にオブジェクトを非アクティブ化してプールに返す処理。
    public void ReturnObject(GameObject _obj, int _time)
    {
        StartCoroutine(IEnumerator_ReturnObject(_obj, _time));
    }

    // オブジェクトを返却する非同期処理本体。
    public IEnumerator IEnumerator_ReturnObject(GameObject _obj, int _time)
    {
        // 指定フレーム数待機する
        for (int i = 0; i < _time; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        // オブジェクトを非アクティブにしてプールに戻す
        _obj.SetActive(false);
        pool.Enqueue(_obj);
        _obj.transform.SetParent(transform);
    }
}
