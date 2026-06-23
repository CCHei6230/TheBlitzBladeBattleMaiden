using UnityEngine;
using System.Collections.Generic;

// 音声クリップを一元管理するシングルトンクラス。
// AudioClip ごとに AudioSource を動的に生成し、名前をキーとして Dictionary で管理する。
// シーン遷移後もオブジェクトが保持される（DontDestroyOnLoad）。
public class AudioClipManager : MonoBehaviour
{
    // Inspector で登録する音声クリップの一覧
    [SerializeField]
    private List<AudioClip> audioClipSuper;

    // クリップ名をキーとした AudioSource の Dictionary
    [SerializeReference]
    private Dictionary<string, AudioSource> audioSourceDic;

    // シングルトンのインスタンス参照
    static public AudioClipManager instance = null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        // このオブジェクトをシングルトンのインスタンスとして登録する
        instance = this;
        // シーン遷移時にこのオブジェクトを破棄しない
        DontDestroyOnLoad(gameObject);

        audioSourceDic = new Dictionary<string, AudioSource>();
        foreach (var item in audioClipSuper)
        {
            // クリップごとに AudioSource コンポーネントをアタッチする
            AudioSource tmp_Audio = gameObject.AddComponent<AudioSource>();
            // AudioSource にクリップを割り当てる
            tmp_Audio.clip = item;
            // クリップ名をキーとして Dictionary に登録する
            audioSourceDic.Add(item.name, tmp_Audio);
        }
    }

    // 指定したキーの音声クリップを再生する。
    // 同じクリップが再生中の場合は音量を下げて重ね再生する。
    public void PlayAudioOneShot(string _key)
    {
        if (audioSourceDic.ContainsKey(_key))
        {
            if (audioSourceDic[_key].isPlaying)
            {
                audioSourceDic[_key].volume = 0.6f;
            }
            else
            {
                audioSourceDic[_key].volume = 1;
            }
            audioSourceDic[_key].PlayOneShot(audioSourceDic[_key].clip);
        }
    }

    // 指定したキーの音声クリップが再生中でない場合のみ再生する。
    // 同じ音が重複して再生されるのを防ぎたい場合に使用する。
    public void PlayAudioOneShotWhileNotPlaying(string _key)
    {
        if (audioSourceDic.ContainsKey(_key))
        {
            if (audioSourceDic[_key].isPlaying)
            {
                return;
            }
            audioSourceDic[_key].PlayOneShot(audioSourceDic[_key].clip);
        }
    }

    // 指定したキーの音声クリップの再生を停止する。
    public void StopAudio(string _key)
    {
        if (audioSourceDic.ContainsKey(_key))
        {
            audioSourceDic[_key].Stop();
        }
    }

    // 指定したキーの音声クリップをループ再生する。
    public void LoopAudio(string _key)
    {
        if (audioSourceDic.ContainsKey(_key))
        {
            audioSourceDic[_key].loop = true;
            audioSourceDic[_key].Play();
        }
    }
}
