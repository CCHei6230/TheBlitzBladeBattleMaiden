using UnityEngine;

// チェックポイントオブジェクト。
// プレイヤーが触れると現在の位置・回転をセーブデータに記録する。
// セーブデータのインデックスと比較し、既に通過済みのチェックポイントは Start 時に自動処理して削除する。
[DefaultExecutionOrder(-998)]
public class CheckPoint : MonoBehaviour
{
    // このチェックポイントに復帰した際に削除するオブジェクトの配列
    public GameObject[] objectsToDestroyWhenLoad;
    // このチェックポイントに復帰した際に Trigger() を呼び出すオブジェクトの配列
    public TriggerableObject[] objectsToTriggerWhenLoad;
    // このチェックポイントでのプレイヤー生成位置
    public Vector3 spawnPosition;
    // このチェックポイントでのプレイヤー生成時の回転
    public Vector3 spawnRotation;
    // このチェックポイントの識別インデックス（小さいほど早い位置）
    public int pointIndex;

    void Start()
    {
        var tmp_checkPointManager = CheckPointManager.instance;

        // セーブデータのインデックスがこのチェックポイントより前を指している場合
        // →まだ到達していないチェックポイントとして、管理リストに登録する
        if (tmp_checkPointManager.saveObject.checkPointIndex < pointIndex)
        {
            tmp_checkPointManager.checkPoints[pointIndex] = this;
        }
        else
        {
            // セーブデータがこのチェックポイント以降を指している場合
            // 既に通過済みとして、復帰時の処理を即時実行してこのオブジェクトを削除する

            // 復帰時に削除すべきオブジェクトを削除する
            foreach (var obj in objectsToDestroyWhenLoad)
            {
                Destroy(obj);
            }
            // 復帰時に Trigger すべきオブジェクトの Trigger() を呼び出す
            foreach (var obj in objectsToTriggerWhenLoad)
            {
                obj.Trigger();
            }
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // プレイヤーがこのチェックポイントに触れた場合
        if (other.tag == "Player")
        {
            // チェックポイントのデータをセーブする
            CheckPointManager.instance.SetSavePositionAndRotation(spawnPosition, spawnRotation, pointIndex);
            Destroy(gameObject);
        }
    }
}
