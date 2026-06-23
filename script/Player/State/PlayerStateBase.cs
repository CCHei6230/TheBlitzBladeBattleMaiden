using UnityEngine;
using DG.Tweening;

// プレイヤーステートの基本クラス
public abstract class PlayerStateBase : StateBase
{
    // プレイヤーのプロパティクラス
    static protected PlayerProperty playerProperty;
    // プレイヤーのプロパティを設定
    public void SetProperty(PlayerProperty _playerProperty)
    {
        if (playerProperty != null) return;
        playerProperty = _playerProperty;
    }
}


// プレイヤーの各ステートの共通処理のクラス
public abstract class PlayerStateSharedMethods
{
    // 移動処理
    public static void Move(PlayerProperty _playerProperty)
    {
        // カメラのTransformを取得
        var tmp_cameraTransform = _playerProperty.cameraTransform;
        // カメラの前方方向(Y軸無視)
        var cameraForward = new Vector3(tmp_cameraTransform.forward.x,0, tmp_cameraTransform.forward.z).normalized; 
        // カメラの右方向(Y軸無視)
        var cameraRight = new Vector3(_playerProperty.cameraTransform.right.x,0, tmp_cameraTransform.right.z).normalized; 
        // 移動方向を計算
        _playerProperty.moveDirection = (cameraForward*_playerProperty.playerInput.moveInputVector.y + cameraRight* _playerProperty.playerInput.moveInputVector.x)* _playerProperty.moveSpeed * Time.fixedDeltaTime ;
        // 速度を適用
        _playerProperty.RB.linearVelocity =new Vector3(_playerProperty.moveDirection.x, _playerProperty.moveDirection.y + _playerProperty.RB.linearVelocity.y,_playerProperty.moveDirection.z); 
        // 前の前方方向を更新
        _playerProperty.lastForward = (cameraForward*_playerProperty.playerInput.moveInputVector.y + cameraRight*_playerProperty.playerInput. moveInputVector.x);
        // モデルを移動方向に向ける
        _playerProperty.modelObject.transform.DORotate(Quaternion.LookRotation(_playerProperty.lastForward).eulerAngles,0.1f);
    }
    // モデルの回転処理
    public static void RotateModel(PlayerProperty _playerProperty , float _rotateTime)
    {
        // カメラのTransformを取得
        var tmp_cameraTransform = _playerProperty.cameraTransform;
        // カメラの前方方向(Y軸無視)
        var cameraForward = new Vector3(tmp_cameraTransform.forward.x,0, tmp_cameraTransform.forward.z).normalized; 
        // カメラの右方向(Y軸無視)
        var cameraRight = new Vector3(_playerProperty.cameraTransform.right.x,0, tmp_cameraTransform.right.z).normalized; 
        // 前の前方方向を更新
        _playerProperty.lastForward = (cameraForward*_playerProperty.playerInput.moveInputVector.y + cameraRight*_playerProperty.playerInput. moveInputVector.x);
        // モデルの回転を実行
        _playerProperty.modelObject.transform.DORotate(Quaternion.LookRotation(_playerProperty.lastForward).eulerAngles,_rotateTime);
    }
    // ロックオンした敵にモデルを回転させる処理
    public static void RotateModelToLockOnedEnemy(PlayerProperty _playerProperty , float _rotateTime)
    {
        // モデルのDO系処理を中止
        _playerProperty.modelObject.transform.DOComplete();
        // ロックオン対象の方向に向く
        _playerProperty.modelObject.transform
            .DOLookAt(_playerProperty.lockOnEnemy.transform.position, _rotateTime, AxisConstraint.Y)
            .OnUpdate(() =>
                _playerProperty.lastForward = _playerProperty.modelObject.transform.forward);
    }

    // 攻撃のFixedUpdateの処理
    public static void AttackFixedUpdate(PlayerProperty _playerProperty )
    {
        // 攻撃判定開始フレームの場合
        if (_playerProperty.atkCount == _playerProperty.atkSObjectCurrent.frameStartCollider)
        {
            // 当たり判定の位置を設定
            _playerProperty.attackCollider.transform.localPosition = _playerProperty.atkSObjectCurrent.colliderPosition;
            // 当たり判定のサイズを設定
            _playerProperty.attackCollider.transform.localScale =_playerProperty.atkSObjectCurrent.colliderSize;
            // 当たり判定を有効に
            _playerProperty.attackCollider.SetActive(true);
        }
        // 攻撃判定終了フレームの場合
        if (_playerProperty.atkCount == _playerProperty.atkSObjectCurrent.frameEndCollider)
        {
            // 当たり判定を無効に
            _playerProperty.attackCollider.SetActive(false);
        }

        // エフェクト生成フレームの場合
        if (_playerProperty.atkCount == _playerProperty.atkSObjectCurrent.frameSpawnEffect[_playerProperty.atkEffectIndex])
        {
            // 攻撃エフェクトを生成
            var tmp_effect =  MonoBehaviour.Instantiate(_playerProperty.atkSObjectCurrent.atkEffect, _playerProperty.modelObject.transform);
            // エフェクトの回転を設定
            tmp_effect.transform.localRotation = Quaternion.Euler(_playerProperty.atkSObjectCurrent.effectRotation[_playerProperty.atkEffectIndex]);
            // エフェクトのサイズを設定
            tmp_effect.transform.localScale = Vector3.one * _playerProperty.atkSObjectCurrent.effectSize[_playerProperty.atkEffectIndex];
            // 親要素を解除
            tmp_effect.transform.parent = null;
            // 4秒後エフェクトを削除
            MonoBehaviour.Destroy(tmp_effect , 4f);
            
            // エフェクトインデックスを更新
            _playerProperty.atkEffectIndex++;
            if (_playerProperty.atkEffectIndex >= _playerProperty.atkSObjectCurrent.frameSpawnEffect.Length)
            {
                _playerProperty.atkEffectIndex = 0;
            }
        }
        // 攻撃カウントを更新
        _playerProperty.atkCount++;

        // 突進移動中の場合
        if (_playerProperty.atkCount > _playerProperty.atkSObjectCurrent.frameStartMoveForward
            && _playerProperty.atkCount < _playerProperty.atkSObjectCurrent.frameEndMoveForward)
        {
            // 移動方向と速度を計算
            Vector3 modelForward = _playerProperty.modelObject.transform.forward;
            Vector3 finalVelocity = modelForward * _playerProperty.atkForwardSpeed*(1-(_playerProperty.atkCount /_playerProperty.atkCountMax-10));
            // 速度を適用
            _playerProperty.RB.linearVelocity = new Vector3(finalVelocity.x,0,finalVelocity.z); 
        }
        // 移動停止
        else
        {
            _playerProperty.RB.linearVelocity = Vector3.zero;
        }

        // 上昇移動中の場合
        if (_playerProperty.atkCount > _playerProperty.atkSObjectCurrent.frameStartMoveUp
            && _playerProperty.atkCount < _playerProperty.atkSObjectCurrent.frameEndMoveUp)
        {
            // 上昇速度を計算
            Vector3 modelUp = _playerProperty.modelObject.transform.up;
            Vector3 finalVelocity = modelUp * _playerProperty.atkSObjectCurrent.risingSpeed*(1-(_playerProperty.atkCount /_playerProperty.atkCountMax-10));
            // Y軸の速度を適用
            _playerProperty.RB.linearVelocity = new Vector3(_playerProperty.RB.linearVelocity.x,finalVelocity.y,_playerProperty.RB.linearVelocity.z); 
        }

        // 下降移動中の場合
        if (_playerProperty.atkCount > _playerProperty.atkSObjectCurrent.frameStartMoveDown
            && _playerProperty.atkCount < _playerProperty.atkSObjectCurrent.frameEndMoveDown)
        {
            // 下降速度を計算
            Vector3 modelUp = _playerProperty.modelObject.transform.up;
            Vector3 finalVelocity = modelUp * _playerProperty.atkSObjectCurrent.fallingSpeed*(_playerProperty.atkCount /_playerProperty.atkCountMax-10);
            // Y軸の速度を適用
            _playerProperty.RB.linearVelocity = new Vector3(_playerProperty.RB.linearVelocity.x,finalVelocity.y,_playerProperty.RB.linearVelocity.z); 
        }
    }

    // 攻撃のデータを設定する処理
    public static void SetAttackData(PlayerProperty _playerProperty  )
    {
        // エフェクトインデックスを初期化
        _playerProperty.atkEffectIndex = 0;
        // 先行入力をfalseに
        _playerProperty.atkPreInput = false;
        // 攻撃レベル上昇の可否を設定
        _playerProperty.playerAttack.canIncreaseAttackLv = _playerProperty.atkSObjectCurrent.canIncreaseAttackLv;
        // 攻撃カウント最大値を設定
        _playerProperty.atkCountMax = _playerProperty.atkSObjectCurrent.duration;
        // 攻撃カウントを初期化
        _playerProperty.atkCount = 0;
        // 攻撃力を設定
        _playerProperty.playerAttack.damage =_playerProperty.atkSObjectCurrent.damage;
        // 前進速度を設定
        _playerProperty.atkForwardSpeed =_playerProperty.atkSObjectCurrent.forwardSpeed;
        // 攻撃レベルが1より大きい場合
        if (_playerProperty.playerAttackLv > 1)
        {
            // 攻撃レベルに応じてダメージを加算
            _playerProperty.playerAttack.damage +=( _playerProperty.playerAttackLv*5);
        }
    }

    // 残像を生成する処理
    public static void SpawnMeshTrail(ObjectPool _objectPool,PlayerProperty _playerProperty,int _activeTime,int _refreshTime,int _destroyTime )
    {

        // SkinnedMeshRenderer用
        _playerProperty.StartCoroutine(
            MeshTrail.IEnumerator_TrailSkinnedMesh(_objectPool,_playerProperty.modelObject,_activeTime,_refreshTime,
                _destroyTime,_playerProperty.trailMaterial,"_EmissionColor"));

        // MeshRenderer用
        _playerProperty.StartCoroutine(
            MeshTrail.IEnumerator_Trail(_objectPool,_playerProperty.modelObject,_activeTime,_refreshTime,
                _destroyTime,_playerProperty.trailMaterial,"_EmissionColor"));


    }

    // ダメージを受ける処理
    public static StateBase TakeDamage(PlayerProperty _playerProperty,Collider _other, StateBase _state)
    {
        // ダメージインターフェースを取得
        var  tmp_damage = _other.GetComponentInParent<IDamageable>();
        if (tmp_damage != null)
        {
            // 受けたダメージ量を設定
            _playerProperty.takeDamage = tmp_damage.Damage;
            // ダメージオブジェクトの位置を設定
            _playerProperty.damagePosition = _other.transform.position;
            // damage stateに遷移
            return  _playerProperty.stateDictionary[PlayerState.damage];
        }
        return _state;
    }
}
