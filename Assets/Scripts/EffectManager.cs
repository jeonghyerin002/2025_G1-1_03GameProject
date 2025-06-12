using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("����Ʈ �����յ�")]
    public GameObject mergeSuccessEffect;    // �ռ� ���� ����Ʈ
    public GameObject mergeFailEffect;       // �ռ� ���� ����Ʈ
    public GameObject deleteEffect;          // ���� ����Ʈ
    public GameObject winEffect;             // �¸� ����Ʈ

    [Header("���� ���� ����Ʈ")]
    public GameObject gameEndEffect;         // ���� ���� ����Ʈ

    [Header("����Ʈ ����")]
    public float effectDuration = 2f;        // ����Ʈ ���� �ð�

    private GameManager gameManager;

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
        gameManager = FindObjectOfType<GameManager>(); // ���ӸŴ��� ã��
        TurnOffAllEffects();
    }


    public void PlayGameEndEffect(Vector3 position)
    {
        PlayEffect(gameEndEffect, position);
    }


    // �ռ� ���� ����Ʈ ���
    public void PlayMergeSuccessEffect(Vector3 position)
    {
        PlayEffect(mergeSuccessEffect, position);
    }

    // �ռ� ���� ����Ʈ ���
    public void PlayMergeFailEffect(Vector3 position)
    {
        PlayEffect(mergeFailEffect, position);
    }

    // ���� ����Ʈ ���
    public void PlayDeleteEffect(Vector3 position)
    {
        PlayEffect(deleteEffect, position);
    }

    // �¸� ����Ʈ ���
    public void PlayWinEffect(Vector3 position)
    {
        PlayEffect(winEffect, position);
    }


    // ��� ����Ʈ ����
    void TurnOffAllEffects()
    {
        if (mergeSuccessEffect != null) mergeSuccessEffect.SetActive(false);
        if (mergeFailEffect != null) mergeFailEffect.SetActive(false);
        if (deleteEffect != null) deleteEffect.SetActive(false);
        if (winEffect != null) winEffect.SetActive(false);
    }

    // PlayEffect �Լ� ���� (����Ʈ ������ ��)
    void PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            // ����Ʈ ���� �˸�
            if (gameManager != null)
                gameManager.SetAnyEffectPlaying(true);

            effectPrefab.SetActive(true);
            StartCoroutine(TurnOffEffectAfterTime(effectPrefab, effectDuration));
        }
    }

    // TurnOffEffectAfterTime ���� (����Ʈ ���� ��)
    IEnumerator TurnOffEffectAfterTime(GameObject effect, float time)
    {
        yield return new WaitForSeconds(time);

        if (effect != null)
        {
            effect.SetActive(false);
        }

        // ����Ʈ �����ٰ� �˸�
        if (gameManager != null)
            gameManager.SetAnyEffectPlaying(false);
    }
}