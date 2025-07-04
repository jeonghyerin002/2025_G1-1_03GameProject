using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [Header("카드 데이터 - 새로 추가")]
    public CardInfoSO cardInfo; // SO 데이터

    [Header("기존 데이터 - 호환용")]
    public int cardValue;
    public Sprite cardImage;
    public TextMeshPro cardText;

    [Header("카드 등급")]
    public CardEdition cardEdition = CardEdition.REGULAR;

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

    // SO 기반 초기화 - 새로 추가
    public void InitCard(CardInfoSO info)
    {
        cardInfo = info;
        cardValue = info.gameValue;
        cardImage = info.cardSprite;

        // 기존 방식으로 초기화
        InitCard(cardValue, cardImage);
    }

    public void InitCard(int value, Sprite image)
    {
        cardValue = value;
        cardImage = image;

        GetComponent<SpriteRenderer>().sprite = image;

        if (cardText != null)
        {
            // SO 데이터가 있으면 카드 이름 사용, 없으면 기존 방식
            if (cardInfo != null)
            {
                cardText.text = GetCardDisplayName();
            }
            else
            {
                cardText.text = cardValue.ToString();
            }
        }

        ApplyValueBasedEffect();

        if (shaderEffect != null)
        {
            shaderEffect.SetEditionByCardValue();
        }
    }

    string GetCardDisplayName()
    {
        if (cardInfo == null) return cardValue.ToString();

        string suitSymbol = GetSuitSymbol();
        string numberText = GetNumberText();

        return suitSymbol + numberText;
    }

    string GetSuitSymbol()
    {
        switch (cardInfo.suit)
        {
            case CardSuit.Spade: return "♠";
            case CardSuit.Heart: return "♥";
            case CardSuit.Diamond: return "♦";
            case CardSuit.Club: return "♣";
            default: return "";
        }
    }

    string GetNumberText()
    {
        switch (cardInfo.number)
        {
            case 1: return "A";
            case 11: return "J";
            case 12: return "Q";
            case 13: return "K";
            default: return cardInfo.number.ToString();
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
        if (isPlayingCardEffect) return;

        if (visualEffects != null)
        {
            visualEffects.PlayHoverEffect();
        }
    }

    public void StopHoverEffect()
    {
        if (isPlayingCardEffect) return;

        if (visualEffects != null)
        {
            visualEffects.StopHoverEffect();
        }
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

    // 등급 정보 가져오기
    public CardEdition GetCardEdition()
    {
        return cardEdition;
    }

    // 등급 설정 (쉐이더 효과도 함께 적용)
    public void SetCardEdition(CardEdition edition)
    {
        cardEdition = edition;

        // 쉐이더 효과에 등급 적용
        if (shaderEffect != null)
        {
            shaderEffect.SetEdition(edition.ToString());
        }

        Debug.Log($"카드 등급 설정: {edition}");
    }


    // 등급 업그레이드
    public void UpgradeEdition()
    {
        if (CardRaritySystem.Instance != null)
        {
            CardEdition newEdition = CardRaritySystem.Instance.UpgradeEdition(cardEdition);
            SetCardEdition(newEdition);
            Debug.Log($"등급 업그레이드: {cardEdition} -> {newEdition}");
        }
    }

    // 등급 이름 가져오기
    public string GetEditionName()
    {
        switch (cardEdition)
        {
            case CardEdition.REGULAR: return "일반";
            case CardEdition.POLYCHROME: return "레어";
            case CardEdition.NEGATIVE: return "전설";
            default: return "일반";
        }
    }
}