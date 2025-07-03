using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static MergeChanceSO;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public int cardValue;
    public Sprite cardImage;
    public TextMeshPro cardText;
    public MergeChanceSO mergeData;

    [Header("Material Effects")]
    public Material originalMaterial;
    public Material burnMaterial;
    public Material disolveMaterial;
    public Material explosionMaterial;
    public Material shinyMaterial;
    public Material fireMaterial;
    public Material pixelMaterial;

    private SpriteRenderer spriteRenderer;
    private Material currentMaterial;
    private GameManager gameManager;
    private bool isPlayingCardEffect = false;

    // 새로 추가된 비주얼 효과 컴포넌트
    private SimpleCardVisualEffects visualEffects;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            currentMaterial = originalMaterial;
        }

        gameManager = FindObjectOfType<GameManager>();

        // 비주얼 효과 컴포넌트 추가
        visualEffects = GetComponent<SimpleCardVisualEffects>();
        if (visualEffects == null)
        {
            visualEffects = gameObject.AddComponent<SimpleCardVisualEffects>();
        }
    }

    public void OnEnable()
    {
        gameObject.transform.DOPunchRotation(new Vector3(1.05f, 0.05f, 0.05f), 0.5f);
    }

    public void InitCard(int value, Sprite image)
    {
        cardValue = value;
        cardImage = image;

        GetComponent<SpriteRenderer>().sprite = image;

        if (cardText != null)
        {
            cardText.text = cardValue.ToString();
        }

        ApplyValueBasedEffect();
    }

    void ApplyValueBasedEffect()
    {
        if (cardValue >= 10)
        {
            ApplyShinyEffect();
        }
    }

    #region 셰이더 효과 함수들 (기존과 동일)

    public void ApplyShinyEffect()
    {
        Debug.Log($"ApplyShinyEffect 호출됨!");

        if (shinyMaterial != null && spriteRenderer != null)
        {
            Debug.Log("ShinyMaterial 적용 중...");
            spriteRenderer.material = shinyMaterial;
            currentMaterial = shinyMaterial;
        }
        else
        {
            Debug.LogError($"ShinyMaterial이 null입니다: {shinyMaterial == null}, SpriteRenderer가 null입니다: {spriteRenderer == null}");
        }
    }

    public void ApplyBurnEffect(float burnValue = 0.5f)
    {
        if (burnMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = burnMaterial;
            currentMaterial = spriteRenderer.material;

            if (currentMaterial.HasProperty("Burn_Value"))
            {
                currentMaterial.SetFloat("Burn_Value", burnValue);
            }
        }
    }

    public void ApplyDisolveEffect(float disolveValue = 0.5f)
    {
        if (disolveMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = disolveMaterial;
            currentMaterial = spriteRenderer.material;

            if (currentMaterial.HasProperty("Disolve_Value"))
            {
                currentMaterial.SetFloat("Disolve_Value", disolveValue);
            }
        }
    }

    public void ApplyExplosionEffect(float explosionValue = 1f)
    {
        if (explosionMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = explosionMaterial;
            currentMaterial = spriteRenderer.material;

            if (currentMaterial.HasProperty("ExplosionValue"))
            {
                currentMaterial.SetFloat("ExplosionValue", explosionValue);
            }
        }
    }

    public void RestoreOriginalMaterial()
    {
        if (originalMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
            currentMaterial = originalMaterial;
        }
    }

    #endregion

    #region 기존 애니메이션과 함께하는 셰이더 효과 (기존과 동일)

    public void PlayMergeSuccessEffect()
    {
        if (isPlayingCardEffect) return;
        StartCoroutine(MergeSuccessSequence());
    }

    IEnumerator MergeSuccessSequence()
    {
        isPlayingCardEffect = true;

        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null) dragDrop.enabled = false;

        ApplyDisolveEffect(0f);

        float disolveTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < disolveTime)
        {
            elapsedTime += Time.deltaTime;
            float disolveValue = Mathf.Lerp(0f, 1f, elapsedTime / disolveTime);

            if (currentMaterial != null && currentMaterial.HasProperty("Disolve_Value"))
            {
                currentMaterial.SetFloat("Disolve_Value", disolveValue);
            }
            yield return null;
        }

        transform.DOShakePosition(0.5f, strength: 20f);

        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);

        isPlayingCardEffect = false;
    }

    public void PlayMergeFailureEffect()
    {
        if (isPlayingCardEffect) return;
        StartCoroutine(MergeFailureSequence());
    }

    IEnumerator MergeFailureSequence()
    {
        isPlayingCardEffect = true;

        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null) dragDrop.enabled = false;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.DOColor(Color.red, 0.2f).SetLoops(4, LoopType.Yoyo);

        yield return new WaitForSeconds(0.8f);

        ApplyDisolveEffect(0f);

        float disolveTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < disolveTime)
        {
            elapsedTime += Time.deltaTime;
            float disolveValue = Mathf.Lerp(0f, 1f, elapsedTime / disolveTime);

            if (currentMaterial != null && currentMaterial.HasProperty("Disolve_Value"))
            {
                currentMaterial.SetFloat("Disolve_Value", disolveValue);
            }
            yield return null;
        }

        transform.DOShakePosition(0.5f, strength: 20f);

        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);

        isPlayingCardEffect = false;
    }

    public void PlayDeleteEffect()
    {
        if (isPlayingCardEffect) return;
        StartCoroutine(DeleteSequence());
    }

    IEnumerator DeleteSequence()
    {
        isPlayingCardEffect = true;

        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null) dragDrop.enabled = false;

        ApplyExplosionEffect(0f);

        float explosionTime = 0.6f;
        float elapsedTime = 0f;

        while (elapsedTime < explosionTime)
        {
            elapsedTime += Time.deltaTime;
            float explosionValue = Mathf.Lerp(0f, 1f, elapsedTime / explosionTime);

            if (currentMaterial != null && currentMaterial.HasProperty("ExplosionValue"))
            {
                currentMaterial.SetFloat("ExplosionValue", explosionValue);
            }
            yield return null;
        }

        transform.DOMoveY(transform.position.y + 3f, 0.5f);
        spriteRenderer.DOFade(0f, 0.5f);

        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);

        isPlayingCardEffect = false;
    }

    // 호버 효과 (새로운 비주얼 효과 추가)
    public void PlayHoverEffect()
    {
        if (isPlayingCardEffect ||
            (gameManager != null && !gameManager.CanInteract()))
        {
            return;
        }

        ApplyShinyEffect();

        // 새로운 비주얼 효과 추가
        if (visualEffects != null)
        {
            visualEffects.PlayHoverEffect();
        }
    }

    // 호버 해제 (새로운 비주얼 효과 추가)
    public void StopHoverEffect()
    {
        if (isPlayingCardEffect)
        {
            return;
        }

        RestoreOriginalMaterial();

        // 새로운 비주얼 효과 추가
        if (visualEffects != null)
        {
            visualEffects.StopHoverEffect();
        }
    }

    // 스왑 효과 추가
    public void PlaySwapEffect(float direction = 1f)
    {
        if (visualEffects != null)
        {
            visualEffects.PlaySwapEffect(direction);
        }
    }

    #endregion

    #region 마우스 이벤트 (기존과 동일)

    void OnMouseEnter()
    {
        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null && !dragDrop.isDragging)
        {
            PlayHoverEffect();
        }
    }

    void OnMouseExit()
    {
        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null && !dragDrop.isDragging)
        {
            StopHoverEffect();
        }
    }

    #endregion

    public bool IsPlayingEffect()
    {
        return isPlayingCardEffect;
    }
}