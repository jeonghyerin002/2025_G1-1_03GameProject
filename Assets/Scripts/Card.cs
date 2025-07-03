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

    // ���� �߰��� ���־� ȿ�� ������Ʈ
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

        // ���־� ȿ�� ������Ʈ �߰�
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

    #region ���̴� ȿ�� �Լ��� (������ ����)

    public void ApplyShinyEffect()
    {
        Debug.Log($"ApplyShinyEffect ȣ���!");

        if (shinyMaterial != null && spriteRenderer != null)
        {
            Debug.Log("ShinyMaterial ���� ��...");
            spriteRenderer.material = shinyMaterial;
            currentMaterial = shinyMaterial;
        }
        else
        {
            Debug.LogError($"ShinyMaterial�� null�Դϴ�: {shinyMaterial == null}, SpriteRenderer�� null�Դϴ�: {spriteRenderer == null}");
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

    #region ���� �ִϸ��̼ǰ� �Բ��ϴ� ���̴� ȿ�� (������ ����)

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

    // ȣ�� ȿ�� (���ο� ���־� ȿ�� �߰�)
    public void PlayHoverEffect()
    {
        if (isPlayingCardEffect ||
            (gameManager != null && !gameManager.CanInteract()))
        {
            return;
        }

        ApplyShinyEffect();

        // ���ο� ���־� ȿ�� �߰�
        if (visualEffects != null)
        {
            visualEffects.PlayHoverEffect();
        }
    }

    // ȣ�� ���� (���ο� ���־� ȿ�� �߰�)
    public void StopHoverEffect()
    {
        if (isPlayingCardEffect)
        {
            return;
        }

        RestoreOriginalMaterial();

        // ���ο� ���־� ȿ�� �߰�
        if (visualEffects != null)
        {
            visualEffects.StopHoverEffect();
        }
    }

    // ���� ȿ�� �߰�
    public void PlaySwapEffect(float direction = 1f)
    {
        if (visualEffects != null)
        {
            visualEffects.PlaySwapEffect(direction);
        }
    }

    #endregion

    #region ���콺 �̺�Ʈ (������ ����)

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