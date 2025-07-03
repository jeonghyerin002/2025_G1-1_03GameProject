using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardShaderEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;  // UI Image 대신 SpriteRenderer 사용
    private Material m;
    private Card cardComponent;

    [Header("셰이더 효과 설정")]
    public bool enableShaderEffect = true;
    public float rotationSensitivity = 1f;

    void Start()
    {
        // SpriteRenderer 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer가 없습니다!");
            return;
        }

        // 머티리얼 복사해서 사용 (원본 보호)
        if (spriteRenderer.material != null)
        {
            m = new Material(spriteRenderer.material);
            spriteRenderer.material = m;
        }

        // Card 컴포넌트 가져오기
        cardComponent = GetComponent<Card>();

        // 랜덤 에디션 설정
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

        // 기존 키워드 비활성화
        for (int i = 0; i < spriteRenderer.material.enabledKeywords.Length; i++)
        {
            spriteRenderer.material.DisableKeyword(spriteRenderer.material.enabledKeywords[i]);
        }

        // 랜덤 에디션 활성화
        string selectedEdition = editions[Random.Range(0, editions.Length)];
        spriteRenderer.material.EnableKeyword("_EDITION_" + selectedEdition);

        Debug.Log($"카드 에디션: {selectedEdition}");
    }

    void Update()
    {
        if (!enableShaderEffect || m == null) return;

        UpdateShaderRotation();
    }

    void UpdateShaderRotation()
    {
        // SimpleCardVisualEffects가 있다면 그것의 회전을 사용
        SimpleCardVisualEffects visualEffects = GetComponent<SimpleCardVisualEffects>();

        Quaternion currentRotation;
        if (visualEffects != null)
        {
            // 비주얼 효과의 회전 사용
            currentRotation = transform.localRotation;
        }
        else
        {
            // 기본 회전 사용
            currentRotation = transform.localRotation;
        }

        // 쿼터니언을 오일러 각도로 변환
        Vector3 eulerAngles = currentRotation.eulerAngles;

        // X, Y 축 각도 가져오기
        float xAngle = eulerAngles.x;
        float yAngle = eulerAngles.y;

        // 각도를 -90~90 범위로 제한
        xAngle = ClampAngle(xAngle, -90f, 90f);
        yAngle = ClampAngle(yAngle, -90f, 90f);

        // 셰이더에 회전 값 전달
        if (m.HasProperty("_Rotation"))
        {
            Vector2 rotationValue = new Vector2(
                ExtensionMethods.Remap(xAngle * rotationSensitivity, -20, 20, -0.5f, 0.5f),
                ExtensionMethods.Remap(yAngle * rotationSensitivity, -20, 20, -0.5f, 0.5f)
            );

            m.SetVector("_Rotation", rotationValue);
        }
    }

    // 각도를 최소값과 최대값 사이로 제한하는 메서드
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180f)
            angle += 360f;
        if (angle > 180f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    // 특정 에디션으로 강제 설정
    public void SetEdition(string editionName)
    {
        if (m == null) return;

        // 기존 키워드 비활성화
        for (int i = 0; i < spriteRenderer.material.enabledKeywords.Length; i++)
        {
            spriteRenderer.material.DisableKeyword(spriteRenderer.material.enabledKeywords[i]);
        }

        // 새 에디션 활성화
        spriteRenderer.material.EnableKeyword("_EDITION_" + editionName);
    }

    // 카드 값에 따라 자동으로 에디션 설정
    public void SetEditionByCardValue()
    {
        if (cardComponent == null) return;

        string edition = "REGULAR";

        if (cardComponent.cardValue >= 13)       // 스페이드 (13-16)
        {
            edition = "NEGATIVE";
        }
        else if (cardComponent.cardValue >= 9)   // 하트 (9-12)
        {
            edition = "POLYCHROME";
        }
        else                                     // 클로버, 다이아 (1-8)
        {
            edition = "REGULAR";
        }

        SetEdition(edition);
    }
}