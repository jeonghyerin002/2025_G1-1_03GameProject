using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public Material targetMaterial;  // 적용할 머티리얼
    public string propertyName = "_ShadowLight_Intensity_1"; // 애니메이션할 속성 이름
    public float maxIntensity = 4f;  // 최대 값
    public float speed = 1f;         // 반짝이는 속도 (1이면 약 6.28초 주기)

    private void Update()
    {
        if (targetMaterial == null) return;

        // 0 -> 4 -> 0 패턴 생성 (sin 곡선 기반)
        float intensity = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f; // 0 ~ 1
        intensity *= maxIntensity; // 0 ~ 4
        targetMaterial.SetFloat(propertyName, intensity);
    }
}
