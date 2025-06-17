using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;

    public AudioClip cardPlaceClip;    //카드 놓았을 떄
    public AudioClip mergeAreaClip;      //MergeArea에 놓았을 떄
    public AudioClip discardClip;        //카드를 버렸을 떄
    public AudioClip mergeSuccessclip;     //합성 성공
    public AudioClip mergeFailClip;       //합성 실패
    public AudioClip fullWarningClip;    //카드가 꽉 찼을 떄 경고
    public AudioClip roundSuccessClip;  // 라운드 성공
    public AudioClip roundFailClip;       // 라운드 실패

    void Awake()
    {
      if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayCardPlace() => PlaySFX(cardPlaceClip);
    public void PlayMergeArea() => PlaySFX(mergeAreaClip);
    public void PlayDiscard() => PlaySFX(discardClip);
    public void PlayMergeSuccess() => PlaySFX(mergeSuccessclip);
    public void PlayMergeFail() => PlaySFX(mergeFailClip);
    public void PlayFullWarning() => PlaySFX(fullWarningClip);
    public void PlayRoundSuccess() => PlaySFX(roundSuccessClip);
    public void PlayRoundFail() => PlaySFX(roundFailClip);



}
