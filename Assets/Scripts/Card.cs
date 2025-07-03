using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static MergeChanceSO;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [Header("Card Data")]
    public int cardValue;
    public Sprite cardImage;
    public TextMeshPro cardText;
    public MergeChanceSO mergeData;

    [Header("Material Effects (이펙트 연출용)")]
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

    // 새로운 비주얼 시스템 (호버/이동용)
    private SimpleCardVisualEffects visualEffects;
    private CardShaderEffect shaderEffect;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            currentMaterial = originalMaterial;
        }

        gameManager = FindObjectOfType<GameManager>();

        // 새로운 비주얼 효과 컴포넌트 추가
        visualEffects = GetComponent<SimpleCardVisualEffects>();
        if (visualEffects == null)
        {
            visualEffects = gameObject.AddComponent<SimpleCardVisualEffects>();
        }

        // 셰이더 효과 컴포넌트 추가
        shaderEffect = GetComponent<CardShaderEffect>();
        if (shaderEffect == null)
        {
            shaderEffect = gameObject.AddComponent<CardShaderEffect>();
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

        // 카드 값에 따른 기본 효과
        ApplyValueBasedEffect();

        // 셰이더 효과 초기화
        if (shaderEffect != null)
        {
            shaderEffect.SetEditionByCardValue();
        }
    }

    void ApplyValueBasedEffect()
    {
        if (cardValue >= 10)
        {
            // 이제 호버 시에만 shiny 효과 적용
            // 평상시에는 새로운 셰이더 시스템 사용
        }
    }

    #region 기존 머티리얼 이펙트 (연출용 유지)

    public void ApplyShinyEffect()
    {
        if (shinyMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = shinyMaterial;
            currentMaterial = shinyMaterial;
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

            // 셰이더 효과도 원래대로 복원
            if (shaderEffect != null)
            {
                shaderEffect.SetEditionByCardValue();
            }
        }
    }

    #endregion

    #region 기존 이펙트 연출 시퀀스 (그대로 유지)

    public void PlayMergeSuccessEffect()
    {
        if (isPlayingCardEffect) return;
        StartCoroutine(MergeSuccessSequence());
    }

    IEnumerator MergeSuccessSequence()
    {
        isPlayingCardEffect = true;

        // 새로운 비주얼 효과 잠시 비활성화
        if (visualEffects != null)
            visualEffects.enabled = false;
        if (shaderEffect != null)
            shaderEffect.enabled = false;

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

        // 새로운 비주얼 효과 잠시 비활성화
        if (visualEffects != null)
            visualEffects.enabled = false;
        if (shaderEffect != null)
            shaderEffect.enabled = false;

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

        // 새로운 비주얼 효과 잠시 비활성화
        if (visualEffects != null)
            visualEffects.enabled = false;
        if (shaderEffect != null)
            shaderEffect.enabled = false;

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

    #endregion

    #region 새로운 호버/이동 시스템

    public void PlayHoverEffect()
    {
        if (isPlayingCardEffect ||
            (gameManager != null && !gameManager.CanInteract()))
        {
            return;
        }

        // 기존 shiny 효과 대신 새로운 비주얼 효과 사용
        if (visualEffects != null)
        {
            visualEffects.PlayHoverEffect();
        }

        // 셰이더 효과로 빛나는 효과 (선택사항)
        // if (shaderEffect != null)
        // {
        //     shaderEffect.SetEdition("POLYCHROME");
        // }
    }

    public void StopHoverEffect()
    {
        if (isPlayingCardEffect)
        {
            return;
        }

        // 새로운 비주얼 효과 정지
        if (visualEffects != null)
        {
            visualEffects.StopHoverEffect();
        }

        // 원래 셰이더로 복원 (선택사항)
        // if (shaderEffect != null)
        // {
        //     shaderEffect.SetEditionByCardValue();
        // }
    }

    public void PlaySwapEffect(float direction = 1f)
    {
        if (visualEffects != null)
        {
            visualEffects.PlaySwapEffect(direction);
        }
    }

    #endregion

    #region 마우스 이벤트

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