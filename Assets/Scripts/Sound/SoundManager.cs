using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;

    public AudioClip cardPlaceClip;    //ī�� ������ ��
    public AudioClip mergeAreaClip;      //MergeArea�� ������ ��
    public AudioClip discardClip;        //ī�带 ������ ��
    public AudioClip mergeSuccessclip;     //�ռ� ����
    public AudioClip mergeFailClip;       //�ռ� ����
    public AudioClip fullWarningClip;    //ī�尡 �� á�� �� ���
    public AudioClip roundSuccessClip;  // ���� ����
    public AudioClip roundFailClip;       // ���� ����

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
