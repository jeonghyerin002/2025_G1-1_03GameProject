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
    public Material burnMaterial;        // BurnPixel 쉐이더
    public Material disolveMaterial;     // DisolvePixel 쉐이더  
    public Material explosionMaterial;   // PixelExplosion 쉐이더
    public Material shinyMaterial;       // ShinyPixel 쉐이더
    public Material fireMaterial;        // TurnFirePixel 쉐이더
    public Material pixelMaterial;       // Pixelisation 쉐이더

    private SpriteRenderer spriteRenderer;
    private Material currentMaterial;
    private GameManager gameManager;

    // 카드별 연출 상태 관리
    private bool isPlayingCardEffect = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            currentMaterial = originalMaterial;
        }

        // GameManager 참조 획득
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnEnable()
    {
        // 크기 변경 제거! 회전만 남김
        gameObject.transform.DOPunchRotation(new Vector3(1.05f, 0.05f, 0.05f), 0.5f);
    }

    public void MergeData() //데이터를 가져온다
    {

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

        // 카드 값에 따라 기본 효과 적용
        ApplyValueBasedEffect();
    }

    // 카드 값에 따른 기본 효과
    void ApplyValueBasedEffect()
    {
        if (cardValue >= 10) // 높은 값 카드는 반짝이는 효과
        {
            ApplyShinyEffect();
        }
    }

    #region 쉐이더 효과 함수들

    // 반짝이는 효과 (높은 값 카드용)
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

    // 불타는 효과 (머지 성공용)
    public void ApplyBurnEffect(float burnValue = 0.5f)
    {
        Debug.Log($"ApplyBurnEffect 호출됨! burnValue: {burnValue}");

        if (burnMaterial != null && spriteRenderer != null)
        {
            Debug.Log("BurnMaterial 적용 중...");
            spriteRenderer.material = burnMaterial;
            currentMaterial = spriteRenderer.material; // 인스턴스로 가져오기

            // Burn_Value 파라미터 조절
            if (currentMaterial.HasProperty("Burn_Value"))
            {
                currentMaterial.SetFloat("Burn_Value", burnValue);
                Debug.Log($"Burn_Value 설정됨: {burnValue}");
            }
            else
            {
                Debug.LogWarning("Burn_Value 프로퍼티를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"BurnMaterial이 null입니다: {burnMaterial == null}, SpriteRenderer가 null입니다: {spriteRenderer == null}");
        }
    }

    // 소멸 효과 (머지 실패용)
    public void ApplyDisolveEffect(float disolveValue = 0.5f)
    {
        Debug.Log($"ApplyDisolveEffect 호출됨! disolveValue: {disolveValue}");

        if (disolveMaterial != null && spriteRenderer != null)
        {
            Debug.Log("DisolveMaterial 적용 중...");
            spriteRenderer.material = disolveMaterial;
            currentMaterial = spriteRenderer.material; // 인스턴스로 가져오기

            // Disolve_Value 파라미터 조절
            if (currentMaterial.HasProperty("Disolve_Value"))
            {
                currentMaterial.SetFloat("Disolve_Value", disolveValue);
                Debug.Log($"Disolve_Value 설정됨: {disolveValue}");
            }
            else
            {
                Debug.LogWarning("Disolve_Value 프로퍼티를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"DisolveMaterial이 null입니다: {disolveMaterial == null}, SpriteRenderer가 null입니다: {spriteRenderer == null}");
        }
    }

    // 폭발 효과 (삭제용)
    public void ApplyExplosionEffect(float explosionValue = 1f)
    {
        Debug.Log($"ApplyExplosionEffect 호출됨! explosionValue: {explosionValue}");

        if (explosionMaterial != null && spriteRenderer != null)
        {
            Debug.Log("ExplosionMaterial 적용 중...");
            spriteRenderer.material = explosionMaterial;
            currentMaterial = spriteRenderer.material; // 인스턴스로 가져오기

            // ExplosionValue 파라미터 조절
            if (currentMaterial.HasProperty("ExplosionValue"))
            {
                currentMaterial.SetFloat("ExplosionValue", explosionValue);
                Debug.Log($"ExplosionValue 설정됨: {explosionValue}");
            }
            else
            {
                Debug.LogWarning("ExplosionValue 프로퍼티를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"ExplosionMaterial이 null입니다: {explosionMaterial == null}, SpriteRenderer가 null입니다: {spriteRenderer == null}");
        }
    }

    // 불 효과 (특수 카드용)
    public void ApplyFireEffect()
    {
        if (fireMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = fireMaterial;
            currentMaterial = fireMaterial;
        }
    }

    // 픽셀화 효과 (레트로 느낌)
    public void ApplyPixelEffect(float pixelSize = 32f)
    {
        if (pixelMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = pixelMaterial;
            currentMaterial = pixelMaterial;

            // PixelUV_Size_1 파라미터 조절
            if (currentMaterial.HasProperty("PixelUV_Size_1"))
            {
                currentMaterial.SetFloat("PixelUV_Size_1", pixelSize);
            }
        }
    }

    // 원래 상태로 복구
    public void RestoreOriginalMaterial()
    {
        if (originalMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
            currentMaterial = originalMaterial;
        }
    }

    #endregion

    #region 애니메이션과 함께하는 쉐이더 효과

    // 머지 성공 시 연출
    public void PlayMergeSuccessEffect()
    {
        Debug.Log("PlayMergeSuccessEffect 호출됨!");

        if (isPlayingCardEffect)
        {
            Debug.Log("이미 카드 효과가 재생 중입니다.");
            return;
        }

        StartCoroutine(MergeSuccessSequence());
    }

    IEnumerator MergeSuccessSequence()
    {

        isPlayingCardEffect = true;       

       
        ApplyDisolveEffect(0f);

        // Disolve_Value를 서서히 증가
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

        // 3. 흔들리며 사라지기
        transform.DOShakePosition(0.5f, strength: 20f);

        isPlayingCardEffect = false;
       

    }

    // 머지 실패 시 연출
    public void PlayMergeFailureEffect()
    {
        Debug.Log("PlayMergeFailureEffect 호출됨!");

        if (isPlayingCardEffect)
        {
            Debug.Log("이미 카드 효과가 재생 중입니다.");
            return;
        }

        StartCoroutine(MergeFailureSequence());
    }

    IEnumerator MergeFailureSequence()
    {
        Debug.Log("MergeFailureSequence 시작!");
        isPlayingCardEffect = true;

        // 1. 빨간색으로 깜빡임
        Color originalColor = spriteRenderer.color;
        spriteRenderer.DOColor(Color.red, 0.2f).SetLoops(4, LoopType.Yoyo);

        yield return new WaitForSeconds(0.8f);

        // 2. 소멸 효과
        Debug.Log("소멸 효과 시작!");
        ApplyDisolveEffect(0f);

        // Disolve_Value를 서서히 증가
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

        // 3. 흔들리며 사라지기
        transform.DOShakePosition(0.5f, strength: 20f);

        isPlayingCardEffect = false;
        Debug.Log("MergeFailureSequence 완료!");
    }

    // 삭제 시 연출
    public void PlayDeleteEffect()
    {
        Debug.Log("PlayDeleteEffect 호출됨!");

        if (isPlayingCardEffect)
        {
            Debug.Log("이미 카드 효과가 재생 중입니다.");
            return;
        }

        StartCoroutine(DeleteSequence());
    }

    IEnumerator DeleteSequence()
    {
        Debug.Log("DeleteSequence 시작!");
        isPlayingCardEffect = true;

        // 1. 폭발 효과
        Debug.Log("폭발 효과 시작!");
        ApplyExplosionEffect(0f);

        // ExplosionValue를 서서히 증가
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

        // 2. 위로 날아가며 사라지기 (크기 변경 없음)
        transform.DOMoveY(transform.position.y + 3f, 0.5f);
        spriteRenderer.DOFade(0f, 0.5f);

        isPlayingCardEffect = false;
        Debug.Log("DeleteSequence 완료!");
    }

    // 호버 효과 (마우스 올릴 때)
    public void PlayHoverEffect()
    {
        // 연출 중이거나 게임 상호작용이 불가능할 때는 호버 효과 없음
        if (isPlayingCardEffect ||
            (gameManager != null && !gameManager.CanInteract()))
        {
            return;
        }

        ApplyShinyEffect();
    }

    // 호버 해제
    public void StopHoverEffect()
    {
        // 연출 중일 때는 호버 해제하지 않음
        if (isPlayingCardEffect)
        {
            return;
        }

        RestoreOriginalMaterial();
    }

    #endregion

    #region 마우스 이벤트

    void OnMouseEnter()
    {
        // 드래그 중이 아닐 때만 호버 효과 적용
        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null && !dragDrop.isDragging)
        {
            PlayHoverEffect();
        }
    }

    void OnMouseExit()
    {
        // 드래그 중이 아닐 때만 호버 해제
        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null && !dragDrop.isDragging)
        {
            StopHoverEffect();
        }
    }

    #endregion

    // 외부에서 카드 효과 상태 확인용
    public bool IsPlayingEffect()
    {
        return isPlayingCardEffect;
    }
}