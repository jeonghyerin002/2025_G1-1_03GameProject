using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("이펙트 프리팹들")]
    public GameObject mergeSuccessEffect;    // 합성 성공 이펙트
    public GameObject mergeFailEffect;       // 합성 실패 이펙트
    public GameObject deleteEffect;          // 삭제 이펙트
    public GameObject winEffect;             // 승리 이펙트

    [Header("이펙트 설정")]
    public float effectDuration = 2f;        // 이펙트 지속 시간

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 시작할 때 모든 이펙트 끄기
        TurnOffAllEffects();
    }

    // 합성 성공 이펙트 재생
    public void PlayMergeSuccessEffect(Vector3 position)
    {
        PlayEffect(mergeSuccessEffect, position);
    }

    // 합성 실패 이펙트 재생
    public void PlayMergeFailEffect(Vector3 position)
    {
        PlayEffect(mergeFailEffect, position);
    }

    // 삭제 이펙트 재생
    public void PlayDeleteEffect(Vector3 position)
    {
        PlayEffect(deleteEffect, position);
    }

    // 승리 이펙트 재생
    public void PlayWinEffect(Vector3 position)
    {
        PlayEffect(winEffect, position);
    }

    // 이펙트 재생 메인 함수 (켜고 끄기)
    void PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            // 위치 설정하고 켜기
            effectPrefab.transform.position = position;
            effectPrefab.SetActive(true);

            // 시간 후 끄기
            StartCoroutine(TurnOffEffectAfterTime(effectPrefab, effectDuration));
        }
    }

    // 시간 후 이펙트 끄기
    IEnumerator TurnOffEffectAfterTime(GameObject effect, float time)
    {
        yield return new WaitForSeconds(time);

        if (effect != null)
        {
            effect.SetActive(false);
        }
    }

    // 모든 이펙트 끄기
    void TurnOffAllEffects()
    {
        if (mergeSuccessEffect != null) mergeSuccessEffect.SetActive(false);
        if (mergeFailEffect != null) mergeFailEffect.SetActive(false);
        if (deleteEffect != null) deleteEffect.SetActive(false);
        if (winEffect != null) winEffect.SetActive(false);
    }
}