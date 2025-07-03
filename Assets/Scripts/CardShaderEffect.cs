using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardShaderEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;  // UI Image ��� SpriteRenderer ���
    private Material m;
    private Card cardComponent;

    [Header("���̴� ȿ�� ����")]
    public bool enableShaderEffect = true;
    public float rotationSensitivity = 1f;

    void Start()
    {
        // SpriteRenderer ��������
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer�� �����ϴ�!");
            return;
        }

        // ��Ƽ���� �����ؼ� ��� (���� ��ȣ)
        if (spriteRenderer.material != null)
        {
            m = new Material(spriteRenderer.material);
            spriteRenderer.material = m;
        }

        // Card ������Ʈ ��������
        cardComponent = GetComponent<Card>();

        // ���� ����� ����
        SetRandomEdition();
    }

    void SetRandomEdition()
    {
        if (m == null) return;

        string[] editions = new string[4];
        editions[0] = "REGULAR";
        editions[1] = "POLYCHROME";
        editions[2] = "REGULAR";
        editions[3] = "NEGATIVE";

        // ���� Ű���� ��Ȱ��ȭ
        for (int i = 0; i < spriteRenderer.material.enabledKeywords.Length; i++)
        {
            spriteRenderer.material.DisableKeyword(spriteRenderer.material.enabledKeywords[i]);
        }

        // ���� ����� Ȱ��ȭ
        string selectedEdition = editions[Random.Range(0, editions.Length)];
        spriteRenderer.material.EnableKeyword("_EDITION_" + selectedEdition);

        Debug.Log($"ī�� �����: {selectedEdition}");
    }

    void Update()
    {
        if (!enableShaderEffect || m == null) return;

        UpdateShaderRotation();
    }

    void UpdateShaderRotation()
    {
        // SimpleCardVisualEffects�� �ִٸ� �װ��� ȸ���� ���
        SimpleCardVisualEffects visualEffects = GetComponent<SimpleCardVisualEffects>();

        Quaternion currentRotation;
        if (visualEffects != null)
        {
            // ���־� ȿ���� ȸ�� ���
            currentRotation = transform.localRotation;
        }
        else
        {
            // �⺻ ȸ�� ���
            currentRotation = transform.localRotation;
        }

        // ���ʹϾ��� ���Ϸ� ������ ��ȯ
        Vector3 eulerAngles = currentRotation.eulerAngles;

        // X, Y �� ���� ��������
        float xAngle = eulerAngles.x;
        float yAngle = eulerAngles.y;

        // ������ -90~90 ������ ����
        xAngle = ClampAngle(xAngle, -90f, 90f);
        yAngle = ClampAngle(yAngle, -90f, 90f);

        // ���̴��� ȸ�� �� ����
        if (m.HasProperty("_Rotation"))
        {
            Vector2 rotationValue = new Vector2(
                ExtensionMethods.Remap(xAngle * rotationSensitivity, -20, 20, -0.5f, 0.5f),
                ExtensionMethods.Remap(yAngle * rotationSensitivity, -20, 20, -0.5f, 0.5f)
            );

            m.SetVector("_Rotation", rotationValue);
        }
    }

    // ������ �ּҰ��� �ִ밪 ���̷� �����ϴ� �޼���
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180f)
            angle += 360f;
        if (angle > 180f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    // Ư�� ��������� ���� ����
    public void SetEdition(string editionName)
    {
        if (m == null) return;

        // ���� Ű���� ��Ȱ��ȭ
        for (int i = 0; i < spriteRenderer.material.enabledKeywords.Length; i++)
        {
            spriteRenderer.material.DisableKeyword(spriteRenderer.material.enabledKeywords[i]);
        }

        // �� ����� Ȱ��ȭ
        spriteRenderer.material.EnableKeyword("_EDITION_" + editionName);
    }

    // ī�� ���� ���� �ڵ����� ����� ����
    public void SetEditionByCardValue()
    {
        if (cardComponent == null) return;

        string edition = "REGULAR";

        if (cardComponent.cardValue >= 13)       // �����̵� (13-16)
        {
            edition = "NEGATIVE";
        }
        else if (cardComponent.cardValue >= 9)   // ��Ʈ (9-12)
        {
            edition = "POLYCHROME";
        }
        else                                     // Ŭ�ι�, ���̾� (1-8)
        {
            edition = "REGULAR";
        }

        SetEdition(edition);
    }
}