using System.Collections.Generic;
using UnityEngine;

// 敵グループの管理クラス。
// プレイヤーが検知範囲に進入すると敵を生成し、登録された TriggerableObject（結界など）を起動する。
// グループ内の敵が全滅すると、登録された TriggerableObject（結界解除・次のグループやAnchorObject 生成など）を起動してこのオブジェクトを削除する。
// TriggerableObject を継承しており、外部から Trigger() で起動できる。
public class EnemyGroupManager : TriggerableObject
{
    // このグループに属する敵オブジェクトのリスト
    [SerializeReference] List<GameObject> enemies;
    // プレイヤーが検知範囲に進入した際に Trigger() を呼び出すオブジェクトのリスト（結界の展開など）
    public List<TriggerableObject> ObjsToTriggerWhenEnter;
    // グループ内の敵が全滅した際に Trigger() を呼び出すオブジェクトのリスト（結界解除・次のグループやAnchorObject 生成など）
    public List<TriggerableObject> ObjsToTriggerWhenEnd;

    private void Awake()
    {
        foreach (var _enemy in enemies)
        {
            // 各敵にこのグループマネージャーの参照を渡す
            _enemy.GetComponent<EnemyPropertyBase>().groupManager = this;
            // 敵をプレイヤー進入まで非アクティブ状態で待機させる
            _enemy.gameObject.SetActive(false);
        }
    }

    // 敵が倒された際に呼び出される処理。リストから該当の敵を削除し、全滅した場合は終了処理を実行する。
    public void RemoveEnemy(GameObject _enemyToRemove)
    {
        enemies.Remove(_enemyToRemove);

        // グループ内の敵が全滅した場合
        if (enemies.Count <= 0)
        {
            // 全滅時に Trigger すべきオブジェクトの Trigger() を呼び出す
            for (int i = 0; i < ObjsToTriggerWhenEnd.Count; i++)
            {
                if (ObjsToTriggerWhenEnd[i])
                {
                    ObjsToTriggerWhenEnd[i].Trigger();
                }
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが検知範囲に進入した場合
        if (other.CompareTag("Player"))
        {
            // 進入時に Trigger すべきオブジェクトの Trigger() を呼び出す
            for (int i = 0; i < ObjsToTriggerWhenEnter.Count; i++)
            {
                if (ObjsToTriggerWhenEnter[i])
                {
                    ObjsToTriggerWhenEnter[i].Trigger();
                }
            }
            foreach (var _enemy in enemies)
            {
                _enemy.TryGetComponent(out EnemyProperty tmp_enemy);
                if (tmp_enemy)
                {
                    tmp_enemy.playerTransform = other.transform;
                }
                _enemy.gameObject.SetActive(true);
            }
            // 検知範囲のコライダーを無効にして、二重起動を防ぐ
            GetComponent<Collider>().enabled = false;
        }
    }
}
