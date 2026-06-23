using Unity.Cinemachine;
using UnityEngine;

public class PlayerObjectLockOnManager : MonoBehaviour
{
    // プレイヤーのプロパティ
    public PlayerProperty playerProperty;
    // ロックオンUI
    public GameObject  lockOnUI;
    // 敵をロックオンしているか
    bool enemyLockOn = false;
    // プレイヤーカメラ
    public CinemachineCamera  playerCamera;
    // カメラ入力コントローラー
    [SerializeReference] CinemachineInputAxisController  cameraInputAxisController;

    public void UpdateEvent()
    {
        // ワープ攻撃中の場合
        if (playerProperty.warpAttacking)
        {
            //カメラ操作を可能に
            cameraInputAxisController.enabled = true;
            // FOVを調整
            playerCamera.Lens.FieldOfView =Mathf.Lerp(playerCamera.Lens.FieldOfView,70,Time.deltaTime*8.0f);
            lockOnUI.SetActive(false);
            playerProperty.scopeUI.gameObject.SetActive(false);
            //注視対象を自分に設定
            playerCamera.Target.LookAtTarget = transform;
            // ロックオン対象を解除
            playerProperty.lockOnEnemy = null;
            // ロックオンフラグを解除
            enemyLockOn = false;
        }
        else
        {
            // 集中ボタンを押している、かつ入力可能、かつ集中ゲージがある場合
            if (playerProperty.playerInput.ScopeInput.IsPressed() && playerProperty.playerInput.canInput&& playerProperty.focusCount > 0 && playerProperty.Coroutine_FocusReload == null)
            {
                cameraInputAxisController.enabled = true;
                playerProperty.scopeUI.gameObject.SetActive(true);

                lockOnUI.SetActive(false);
                // FOVを狭める
                playerCamera.Lens.FieldOfView =Mathf.Lerp(playerCamera.Lens.FieldOfView,15,Time.deltaTime*15.0f);
                playerCamera.Target.LookAtTarget = transform;
                // ロックオン対象を解除
                playerProperty. lockOnEnemy = null;
                // ロックオンフラグを解除
                enemyLockOn = false;
            }
            else
            {
                playerProperty.scopeUI.gameObject.SetActive(false);

                // ロックオン中の場合
                if (enemyLockOn)
                {
                    cameraInputAxisController.enabled = false;

                    // FOVを調整
                    playerCamera.Lens.FieldOfView =Mathf.Lerp(playerCamera.Lens.FieldOfView,65,Time.deltaTime*5.0f);
                    lockOnUI.SetActive(true);

                    // ロックオン対象が存在する場合
                    if (playerProperty.lockOnEnemy)
                    {
                        // 敵のワールド座標をスクリーン座標に変換
                        var tmp_pos= Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                        lockOnUI.transform.position = new Vector2(tmp_pos.x,  tmp_pos.y);

                        // 敵との距離が45より離れた場合
                        if (Vector3.Distance(playerProperty.lockOnEnemy.position, playerProperty.modelObject.transform.position) > 45)
                        {
                            // 注視対象を自分に戻す
                            playerCamera.Target.LookAtTarget = transform;
                            // ロックオン対象を解除
                            playerProperty.lockOnEnemy = null;
                            // ロックオンフラグを解除
                            enemyLockOn = false;
                        }

                        // ターゲット切り替え入力（左）
                        if (playerProperty.playerInput.LockOnChangeTargetLInput.WasPerformedThisFrame())
                        {
                            Collider[] tmp_hittedEnemy = null;
                            // 周囲の敵をサーチ
                            tmp_hittedEnemy = Physics.OverlapSphere(playerProperty.lockOnEnemy.position, 100, playerProperty.enemyLayer);
                            if (tmp_hittedEnemy != null)
                            {
                                float nearestDistance = Mathf.Infinity;
                                float tmp_distance = 0;
                                Collider tmp_nearestEnemycol = null;
                                foreach (var enemyCol in tmp_hittedEnemy)
                                {
                                    if (enemyCol.transform != playerProperty.lockOnEnemy)
                                    {
                                        var tmp_enemyCamPos = Camera.main.WorldToScreenPoint(enemyCol.transform.position);
                                        var tmp_currentlockOnEdCamPos = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                        // 現在のターゲットより右にいる敵はスキップ
                                        if (tmp_enemyCamPos.x > tmp_currentlockOnEdCamPos.x)
                                        {
                                            continue;
                                        }
                                        tmp_distance = Vector2.Distance(tmp_enemyCamPos, tmp_currentlockOnEdCamPos);
                                        // 最も近い敵を保持
                                        if (tmp_distance < nearestDistance)
                                        {
                                            nearestDistance = tmp_distance;
                                            tmp_nearestEnemycol = enemyCol;
                                        }
                                    }
                                }

                                if (tmp_nearestEnemycol)
                                {
                                    playerProperty.lockOnEnemy = tmp_nearestEnemycol.transform;
                                    var tmp_posNew = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                    lockOnUI.transform.position = new Vector2(tmp_posNew.x,  tmp_posNew.y);
                                    playerCamera.Target.LookAtTarget = playerProperty.lockOnEnemy;
                                }
                            }
                        }

                        // ターゲット切り替え入力（右）
                        if (playerProperty.playerInput.LockOnChangeTargetRInput.WasPerformedThisFrame())
                        {
                            Collider[] tmp_hittedEnemy = Physics.OverlapSphere(playerProperty.lockOnEnemy.position, 100, playerProperty.enemyLayer);
                            if (tmp_hittedEnemy.Length != 0)
                            {
                                float nearestDistance = Mathf.Infinity;
                                float tmp_distance = 0;
                                Collider tmp_nearestEnemycol = null;
                                foreach (var enemyCol in tmp_hittedEnemy)
                                {
                                    if (enemyCol.transform != playerProperty.lockOnEnemy)
                                    {
                                        var tmp_enemyCamPos = Camera.main.WorldToScreenPoint(enemyCol.transform.position);
                                        var tmp_currentlockOnEdCamPos = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                        // 現在のターゲットより左にいる敵はスキップ
                                        if (tmp_enemyCamPos.x < tmp_currentlockOnEdCamPos.x)
                                        {
                                            continue;
                                        }
                                        tmp_distance = Vector2.Distance(tmp_enemyCamPos, tmp_currentlockOnEdCamPos);
                                        if (tmp_distance < nearestDistance)
                                        {
                                            nearestDistance = tmp_distance;
                                            tmp_nearestEnemycol = enemyCol;
                                        }
                                    }
                                }
                                if (tmp_nearestEnemycol)
                                {
                                    playerProperty.lockOnEnemy = tmp_nearestEnemycol.transform;
                                    var tmp_posNew = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                    lockOnUI.transform.position = new Vector2(tmp_posNew.x,  tmp_posNew.y);
                                    playerCamera.Target.LookAtTarget = playerProperty.lockOnEnemy;
                                }
                            }
                        }

                        // ターゲット切り替え入力（上）
                        if (playerProperty.playerInput.LockOnChangeTargetUpInput.WasPerformedThisFrame())
                        {
                            Collider[] tmp_hittedEnemy = Physics.OverlapSphere(playerProperty.lockOnEnemy.position, 100, playerProperty.enemyLayer);
                            if (tmp_hittedEnemy.Length != 0)
                            {
                                float nearestDistance = Mathf.Infinity;
                                float tmp_distance = 0;
                                Collider tmp_nearestEnemycol = null;
                                foreach (var enemyCol in tmp_hittedEnemy)
                                {
                                    if (enemyCol.transform != playerProperty.lockOnEnemy)
                                    {
                                        var tmp_enemyCamPos = Camera.main.WorldToScreenPoint(enemyCol.transform.position);
                                        var tmp_currentlockOnEdCamPos = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                        // 現在のターゲットより下にいる敵はスキップ
                                        if (tmp_enemyCamPos.y < tmp_currentlockOnEdCamPos.y)
                                        {
                                            continue;
                                        }
                                        tmp_distance = Vector2.Distance(tmp_enemyCamPos, tmp_currentlockOnEdCamPos);
                                        if (tmp_distance < nearestDistance)
                                        {
                                            nearestDistance = tmp_distance;
                                            tmp_nearestEnemycol = enemyCol;
                                        }
                                    }
                                }
                                if (tmp_nearestEnemycol)
                                {
                                    playerProperty.lockOnEnemy = tmp_nearestEnemycol.transform;
                                    var tmp_posNew = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                    lockOnUI.transform.position = new Vector2(tmp_posNew.x,  tmp_posNew.y);
                                    playerCamera.Target.LookAtTarget = playerProperty.lockOnEnemy;
                                }
                            }
                        }

                        // ターゲット切り替え入力（下）
                        if (playerProperty.playerInput.LockOnChangeTargetDownInput.WasPerformedThisFrame() && playerProperty.playerInput.canInput)
                        {
                            Collider[] tmp_hittedEnemy = Physics.OverlapSphere(playerProperty.lockOnEnemy.position, 100, playerProperty.enemyLayer);
                            if (tmp_hittedEnemy.Length != 0)
                            {
                                float nearestDistance = Mathf.Infinity;
                                float tmp_distance = 0;
                                Collider tmp_nearestEnemycol = null;
                                foreach (var enemyCol in tmp_hittedEnemy)
                                {
                                    if (enemyCol.transform != playerProperty.lockOnEnemy)
                                    {
                                        var tmp_enemyCamPos = Camera.main.WorldToScreenPoint(enemyCol.transform.position);
                                        var tmp_currentlockOnEdCamPos = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                        // 現在のターゲットより上にいる敵はスキップ
                                        if (tmp_enemyCamPos.y > tmp_currentlockOnEdCamPos.y)
                                        {
                                            continue;
                                        }
                                        tmp_distance = Vector2.Distance(tmp_enemyCamPos, tmp_currentlockOnEdCamPos);
                                        if (tmp_distance < nearestDistance)
                                        {
                                            nearestDistance = tmp_distance;
                                            tmp_nearestEnemycol = enemyCol;
                                        }
                                    }
                                }
                                if (tmp_nearestEnemycol)
                                {
                                    playerProperty.lockOnEnemy = tmp_nearestEnemycol.transform;
                                    var tmp_posNew = Camera.main.WorldToScreenPoint(playerProperty.lockOnEnemy.position);
                                    lockOnUI.transform.position = new Vector2(tmp_posNew.x,  tmp_posNew.y);
                                    playerCamera.Target.LookAtTarget = playerProperty.lockOnEnemy;
                                }
                            }
                        }
                    }
                    else
                    {
                        lockOnUI.SetActive(false);
                        // 注視対象を自分に戻す
                        playerCamera.Target.LookAtTarget = transform;
                        // ロックオン対象を解除
                        playerProperty.lockOnEnemy = null;
                        // ロックオンフラグを解除
                        enemyLockOn = false;
                    }
                }
                else
                {
                    cameraInputAxisController.enabled = true;

                    lockOnUI.SetActive(false);
                    // FOVを元に戻す
                    playerCamera.Lens.FieldOfView =Mathf.Lerp(playerCamera.Lens.FieldOfView,45,Time.deltaTime*5.0f);
                    // ロックオン対象を解除
                    playerProperty. lockOnEnemy = null;
                    // ロックオンフラグを解除
                    enemyLockOn = false;
                    playerCamera.Target.LookAtTarget = transform;
                }

                // ロックオンボタンを押した、かつ入力可能な場合
                if (playerProperty.playerInput.LockOnInput.WasPressedThisFrame()&& playerProperty.playerInput.canInput)
                {
                    // ロックオン中でない場合
                    if (!enemyLockOn)
                    {
                        // 周囲の敵をサーチ
                        Collider[] tmp_hittedEnemy = Physics.OverlapSphere(playerProperty.modelObject.transform.position, 45, playerProperty.enemyLayer);
                        if (tmp_hittedEnemy.Length != 0)
                        {
                            float nearestDistance = 30f;
                            float tmp_distance = 0;
                            playerProperty.lockOnEnemy = null;
                            foreach (var enemyCol in tmp_hittedEnemy)
                            {
                                tmp_distance = Vector2.Distance(
                                    new Vector2(enemyCol.transform.position.x, enemyCol.transform.position.z),
                                    new Vector2(playerProperty.modelObject.transform.position.x,
                                        playerProperty.modelObject.transform.transform.position.z));
                                // 距離が近く、かつ画面内に映っている敵を選択
                                if (tmp_distance < nearestDistance && enemyCol.gameObject.GetComponentInChildren<Renderer>().isVisible)
                                {
                                    nearestDistance = tmp_distance;
                                    playerProperty.lockOnEnemy = enemyCol.transform;
                                }
                            }
                            // ターゲットが見つかった場合
                            if (playerProperty.lockOnEnemy )
                            {
                                playerCamera.Target.LookAtTarget = playerProperty.lockOnEnemy;
                                // ロックオンフラグを立てる
                                enemyLockOn = true;
                            }
                            else
                            {
                                playerProperty.lockOnEnemy = null;
                            }
                        }
                    }
                    // ロックオン中の場合、ロックオンを解除
                    else
                    {
                        // 注視対象を自分に戻す
                        playerCamera.Target.LookAtTarget = transform;
                        // ロックオン対象を解除
                        playerProperty.lockOnEnemy = null;
                        // ロックオンフラグを解除
                        enemyLockOn = false;
                    }
                }
            }
        }
    }
}

