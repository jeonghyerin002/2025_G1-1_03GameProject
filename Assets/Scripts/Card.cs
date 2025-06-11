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
    public Material burnMaterial;        // BurnPixel ���̴�
    public Material disolveMaterial;     // DisolvePixel ���̴�  
    public Material explosionMaterial;   // PixelExplosion ���̴�
    public Material shinyMaterial;       // ShinyPixel ���̴�
    public Material fireMaterial;        // TurnFirePixel ���̴�
    public Material pixelMaterial;       // Pixelisation ���̴�

    private SpriteRenderer spriteRenderer;
    private Material currentMaterial;
    private GameManager gameManager;

    // ī�庰 ���� ���� ����
    private bool isPlayingCardEffect = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            currentMaterial = originalMaterial;
        }

        // GameManager ���� ȹ��
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnEnable()
    {
        // ũ�� ���� ����! ȸ���� ����
        gameObject.transform.DOPunchRotation(new Vector3(1.05f, 0.05f, 0.05f), 0.5f);
    }

    public void MergeData() //�����͸� �����´�
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

        // ī�� ���� ���� �⺻ ȿ�� ����
        ApplyValueBasedEffect();
    }

    // ī�� ���� ���� �⺻ ȿ��
    void ApplyValueBasedEffect()
    {
        if (cardValue >= 10) // ���� �� ī��� ��¦�̴� ȿ��
        {
            ApplyShinyEffect();
        }
    }

    #region ���̴� ȿ�� �Լ���

    // ��¦�̴� ȿ�� (���� �� ī���)
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

    // ��Ÿ�� ȿ�� (���� ������)
    public void ApplyBurnEffect(float burnValue = 0.5f)
    {
        Debug.Log($"ApplyBurnEffect ȣ���! burnValue: {burnValue}");

        if (burnMaterial != null && spriteRenderer != null)
        {
            Debug.Log("BurnMaterial ���� ��...");
            spriteRenderer.material = burnMaterial;
            currentMaterial = spriteRenderer.material; // �ν��Ͻ��� ��������

            // Burn_Value �Ķ���� ����
            if (currentMaterial.HasProperty("Burn_Value"))
            {
                currentMaterial.SetFloat("Burn_Value", burnValue);
                Debug.Log($"Burn_Value ������: {burnValue}");
            }
            else
            {
                Debug.LogWarning("Burn_Value ������Ƽ�� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError($"BurnMaterial�� null�Դϴ�: {burnMaterial == null}, SpriteRenderer�� null�Դϴ�: {spriteRenderer == null}");
        }
    }

    // �Ҹ� ȿ�� (���� ���п�)
    public void ApplyDisolveEffect(float disolveValue = 0.5f)
    {
        Debug.Log($"ApplyDisolveEffect ȣ���! disolveValue: {disolveValue}");

        if (disolveMaterial != null && spriteRenderer != null)
        {
            Debug.Log("DisolveMaterial ���� ��...");
            spriteRenderer.material = disolveMaterial;
            currentMaterial = spriteRenderer.material; // �ν��Ͻ��� ��������

            // Disolve_Value �Ķ���� ����
            if (currentMaterial.HasProperty("Disolve_Value"))
            {
                currentMaterial.SetFloat("Disolve_Value", disolveValue);
                Debug.Log($"Disolve_Value ������: {disolveValue}");
            }
            else
            {
                Debug.LogWarning("Disolve_Value ������Ƽ�� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError($"DisolveMaterial�� null�Դϴ�: {disolveMaterial == null}, SpriteRenderer�� null�Դϴ�: {spriteRenderer == null}");
        }
    }

    // ���� ȿ�� (������)
    public void ApplyExplosionEffect(float explosionValue = 1f)
    {
        Debug.Log($"ApplyExplosionEffect ȣ���! explosionValue: {explosionValue}");

        if (explosionMaterial != null && spriteRenderer != null)
        {
            Debug.Log("ExplosionMaterial ���� ��...");
            spriteRenderer.material = explosionMaterial;
            currentMaterial = spriteRenderer.material; // �ν��Ͻ��� ��������

            // ExplosionValue �Ķ���� ����
            if (currentMaterial.HasProperty("ExplosionValue"))
            {
                currentMaterial.SetFloat("ExplosionValue", explosionValue);
                Debug.Log($"ExplosionValue ������: {explosionValue}");
            }
            else
            {
                Debug.LogWarning("ExplosionValue ������Ƽ�� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError($"ExplosionMaterial�� null�Դϴ�: {explosionMaterial == null}, SpriteRenderer�� null�Դϴ�: {spriteRenderer == null}");
        }
    }

    // �� ȿ�� (Ư�� ī���)
    public void ApplyFireEffect()
    {
        if (fireMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = fireMaterial;
            currentMaterial = fireMaterial;
        }
    }

    // �ȼ�ȭ ȿ�� (��Ʈ�� ����)
    public void ApplyPixelEffect(float pixelSize = 32f)
    {
        if (pixelMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = pixelMaterial;
            currentMaterial = pixelMaterial;

            // PixelUV_Size_1 �Ķ���� ����
            if (currentMaterial.HasProperty("PixelUV_Size_1"))
            {
                currentMaterial.SetFloat("PixelUV_Size_1", pixelSize);
            }
        }
    }

    // ���� ���·� ����
    public void RestoreOriginalMaterial()
    {
        if (originalMaterial != null && spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
            currentMaterial = originalMaterial;
        }
    }

    #endregion

    #region �ִϸ��̼ǰ� �Բ��ϴ� ���̴� ȿ��

    // ���� ���� �� ����
    public void PlayMergeSuccessEffect()
    {
        Debug.Log("PlayMergeSuccessEffect ȣ���!");

        if (isPlayingCardEffect)
        {
            Debug.Log("�̹� ī�� ȿ���� ��� ���Դϴ�.");
            return;
        }

        StartCoroutine(MergeSuccessSequence());
    }

    IEnumerator MergeSuccessSequence()
    {

        isPlayingCardEffect = true;       

       
        ApplyDisolveEffect(0f);

        // Disolve_Value�� ������ ����
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

        // 3. ��鸮�� �������
        transform.DOShakePosition(0.5f, strength: 20f);

        isPlayingCardEffect = false;
       

    }

    // ���� ���� �� ����
    public void PlayMergeFailureEffect()
    {
        Debug.Log("PlayMergeFailureEffect ȣ���!");

        if (isPlayingCardEffect)
        {
            Debug.Log("�̹� ī�� ȿ���� ��� ���Դϴ�.");
            return;
        }

        StartCoroutine(MergeFailureSequence());
    }

    IEnumerator MergeFailureSequence()
    {
        Debug.Log("MergeFailureSequence ����!");
        isPlayingCardEffect = true;

        // 1. ���������� ������
        Color originalColor = spriteRenderer.color;
        spriteRenderer.DOColor(Color.red, 0.2f).SetLoops(4, LoopType.Yoyo);

        yield return new WaitForSeconds(0.8f);

        // 2. �Ҹ� ȿ��
        Debug.Log("�Ҹ� ȿ�� ����!");
        ApplyDisolveEffect(0f);

        // Disolve_Value�� ������ ����
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

        // 3. ��鸮�� �������
        transform.DOShakePosition(0.5f, strength: 20f);

        isPlayingCardEffect = false;
        Debug.Log("MergeFailureSequence �Ϸ�!");
    }

    // ���� �� ����
    public void PlayDeleteEffect()
    {
        Debug.Log("PlayDeleteEffect ȣ���!");

        if (isPlayingCardEffect)
        {
            Debug.Log("�̹� ī�� ȿ���� ��� ���Դϴ�.");
            return;
        }

        StartCoroutine(DeleteSequence());
    }

    IEnumerator DeleteSequence()
    {
        Debug.Log("DeleteSequence ����!");
        isPlayingCardEffect = true;

        // 1. ���� ȿ��
        Debug.Log("���� ȿ�� ����!");
        ApplyExplosionEffect(0f);

        // ExplosionValue�� ������ ����
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

        // 2. ���� ���ư��� ������� (ũ�� ���� ����)
        transform.DOMoveY(transform.position.y + 3f, 0.5f);
        spriteRenderer.DOFade(0f, 0.5f);

        isPlayingCardEffect = false;
        Debug.Log("DeleteSequence �Ϸ�!");
    }

    // ȣ�� ȿ�� (���콺 �ø� ��)
    public void PlayHoverEffect()
    {
        // ���� ���̰ų� ���� ��ȣ�ۿ��� �Ұ����� ���� ȣ�� ȿ�� ����
        if (isPlayingCardEffect ||
            (gameManager != null && !gameManager.CanInteract()))
        {
            return;
        }

        ApplyShinyEffect();
    }

    // ȣ�� ����
    public void StopHoverEffect()
    {
        // ���� ���� ���� ȣ�� �������� ����
        if (isPlayingCardEffect)
        {
            return;
        }

        RestoreOriginalMaterial();
    }

    #endregion

    #region ���콺 �̺�Ʈ

    void OnMouseEnter()
    {
        // �巡�� ���� �ƴ� ���� ȣ�� ȿ�� ����
        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null && !dragDrop.isDragging)
        {
            PlayHoverEffect();
        }
    }

    void OnMouseExit()
    {
        // �巡�� ���� �ƴ� ���� ȣ�� ����
        DragDrop dragDrop = GetComponent<DragDrop>();
        if (dragDrop != null && !dragDrop.isDragging)
        {
            StopHoverEffect();
        }
    }

    #endregion

    // �ܺο��� ī�� ȿ�� ���� Ȯ�ο�
    public bool IsPlayingEffect()
    {
        return isPlayingCardEffect;
    }
}