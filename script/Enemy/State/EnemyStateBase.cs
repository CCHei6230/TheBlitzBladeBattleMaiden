using UnityEngine;

// エネミーステートの基本クラス
public  class EnemyStateBase : StateBase
{
    // エネミーのプロパティクラス
      protected EnemyProperty EnemyProperty;
    // エネミーのプロパティを設定
    public void SetProperty(EnemyProperty enemyProperty)
    {
        // EnemyPropertyが既に存在する場合、処理を抜ける
        if (EnemyProperty != null) return;
        EnemyProperty = enemyProperty;
    }
}

// エネミーの各ステートの共通処理のクラス
public abstract class EnemySharedMethods
{
    // プレイヤーが検知範囲内にいるかチェックする処理
    public static StateBase PlayerInDetectDistance(EnemyProperty enemyProperty, StateBase _state)
    {
        enemyProperty.currentDistanceToPlayer = Vector3.Distance(enemyProperty.modelObject.transform.position,
            enemyProperty.playerTransform.position);

        // 検知範囲内の場合
        if (enemyProperty.currentDistanceToPlayer <= enemyProperty.detectDistanceToPlayer)
        {
            // 攻撃範囲内の場合
            if(enemyProperty.currentDistanceToPlayer  <  enemyProperty.distanceToAttack)
            {
                // attack stateに遷移
                _state = enemyProperty.stateDictionary[EnemyState.attack];
            }
            // 攻撃範囲外だが検知範囲内の場合
            else
            {
                // approach stateに遷移
                _state = enemyProperty.stateDictionary[EnemyState.approach];
            }
        }
        // 検知範囲外の場合
        else
        {
            // standby stateに遷移
            _state = enemyProperty.stateDictionary[EnemyState.standby];
        }
        return _state;
    }
}