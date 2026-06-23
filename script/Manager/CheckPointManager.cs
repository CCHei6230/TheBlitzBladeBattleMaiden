using UnityEngine;
using UnityEngine.Serialization;

// チェックポイントの進行状況を管理するシングルトンクラス。
// プレイヤーの生成位置・回転・通過済みチェックポイントのインデックスを保持し、
// シーン遷移後も情報を維持する（DontDestroyOnLoad）。
[DefaultExecutionOrder(-999)]
public class CheckPointManager : MonoBehaviour
{
    // シングルトンのインスタンス参照
    public static CheckPointManager instance = null;
    // 最新のチェックポイント情報を保持するセーブオブジェクト
    public SaveObject saveObject;
    // プレイヤーの現在の生成位置
    public Vector3 spawnPosition;
    // プレイヤーの現在の生成時の回転
    public Vector3 spawnRotation;
    // ゲーム開始時の初期生成位置
    [SerializeField] Vector3 originSpawnPosition;
    // ゲーム開始時の初期生成回転
    [SerializeField] Vector3 originSpawnRotation;
    // シーン内に存在するチェックポイントの配列（インデックスで管理）
    public CheckPoint[] checkPoints;
    // 最後に通過したチェックポイントのインデックス（-1 は未通過）
    public int checkPointIndex = -1;

    void Awake()
    {
        // シングルトンパターン：既存のインスタンスがあれば自身を破棄する
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // セーブデータが存在しない場合は新規作成する
        if (saveObject == null)
        {
            saveObject = new SaveObject();
            // 初期の生成位置・回転・インデックスをセーブデータに記録する
            saveObject.spawnPosition = spawnPosition;
            saveObject.spawnRotation = spawnRotation;
            saveObject.checkPointIndex = checkPointIndex;
        }
        else
        {
            // セーブデータが存在する場合は、保存された値を読み込む
            spawnPosition = saveObject.spawnPosition;
            spawnRotation = saveObject.spawnRotation;
            checkPointIndex = saveObject.checkPointIndex;
        }
    }

    // インゲーム終了時の処理。セーブデータと各フィールドを初期状態にリセットする。
    public void EndGame()
    {
        saveObject.checkPointIndex = -1;
        checkPointIndex = -1;
        spawnPosition = originSpawnPosition;
        spawnRotation = originSpawnRotation;
        saveObject.spawnPosition = originSpawnPosition;
        saveObject.spawnRotation = originSpawnRotation;
    }

    // チェックポイント通過時の処理。生成位置・回転・インデックスをマネージャーとセーブデータの両方に記録する。
    public void SetSavePositionAndRotation(Vector3 _spawnPosition, Vector3 _spawnRotation, int _index)
    {
        saveObject.spawnPosition = _spawnPosition;
        saveObject.spawnRotation = _spawnRotation;
        saveObject.checkPointIndex = _index;
        spawnPosition = _spawnPosition;
        spawnRotation = _spawnRotation;
        checkPointIndex = _index;
    }
}
