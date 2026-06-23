using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_ImageFillObject : MonoBehaviour
{
    // Image Component
    [SerializeField]Image image;
    // Imageの本来の色
    public Color originalColor ;

    void Start()
    {
        // fillAmountを1にする
        image.fillAmount = 1;
        //originalColorをImageの本来の色に設定
        originalColor = image.color;
    }


    // DOFade処理を使用し、画像をフェードイン後フェードアウトさせる処理
    // _alpha　不透明値
    // _duration　処理の経過時間
    // _ease　イージーの種類
    public void DoFadeInOut(float _alpha,float _duration,Ease _ease = Ease.Linear)
    {
        // imageのDO系処理を完成させる
        image.DOComplete();

        //フェード処理(フェードイン)を実行し、完成後フェード処理(フェードアウト)を実行
        image.DOFade(_alpha,_duration/2f).SetEase(_ease)
            .OnComplete(()=>image.DOFade(0,_duration/2f));
    }


    // DOFillAmount処理を使用し、画像のfillAmountの変動を演出入れる処理
    // _amount　値
    // _maxAmout　値の最大値
    // _duration　処理の経過時間
    // _ease　イージーの種類
    public void DoFillAmount(float _amount, float _maxAmout,float _duration,Ease _ease = Ease.Linear)
    {
        // imageのDO系処理を完成させる
        image.DOComplete();
        // fillAmountの変動を演出入れる処理
        image.DOFillAmount(_amount / _maxAmout, _duration).SetEase(_ease);
    }

    // imageのfillAmountを設定する処理
    public void FillAmount(float _amount, float _maxAmout)
    {
        image.fillAmount = _amount/_maxAmout;
    }

    // imageとoriginalColorの色を設定する処理
    public void SetColor(Color _ColorTo)
    {
        originalColor = _ColorTo;
        image.color = _ColorTo;
    }

    // DOColor処理を使用し,画像の色を変化させた後、また本来の色に戻す処理
    public void DoColorToAndBack(Color _ColorTo, float _duration)
    {
        image.DOColor(_ColorTo,_duration/2f)
            .OnComplete(()=>image.DOColor(originalColor,_duration/2f));
    }
}
