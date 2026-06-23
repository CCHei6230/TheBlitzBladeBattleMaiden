using UnityEngine;

// プレイヤーの進行状況を保存するデータクラス。
// CheckPointManager が保持し、チェックポイント通過時に更新される。
public class SaveObject
{
    // プレイヤーの生成位置
    public Vector3 spawnPosition;
    // プレイヤーの生成時の向き（Euler 角）
    public Vector3 spawnRotation;
    // 最後に通過したチェックポイントのインデックス（-1 は未通過）
    public int checkPointIndex = -1;
}
