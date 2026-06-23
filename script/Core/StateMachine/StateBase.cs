using UnityEngine;

// すべてのステートの基底クラス。各ステートは Enter・Update・FixedUpdate・Exit・Trigger イベントを持つ。
public abstract class StateBase
{
    // 次に遷移するステート
    public StateBase nextState;
    // ステートへの入場時に一度だけ呼び出される処理
    public virtual void StateEnterEvent() {  }
    // 毎フレーム呼び出される処理。次のステートを返す。
    public virtual StateBase UpdateEvent(){ return this;}
    // 毎 FixedUpdate フレームに呼び出される処理。次のステートを返す。
    public virtual StateBase FixedUpdateEvent(){return this; }
    // ステートからの退出時に一度だけ呼び出される処理
    public virtual void ExitEvent(){}
    // コライダーのトリガー領域に進入した際に呼び出される処理
    public virtual void TriggerEnterEvent(Collider other){ }
    // コライダーのトリガー領域に滞在中に呼び出される処理
    public virtual void TriggerStayEvent(Collider other){ }
    // コライダーのトリガー領域から退出した際に呼び出される処理
    public virtual void TriggerExitEvent(Collider other){ }

}
