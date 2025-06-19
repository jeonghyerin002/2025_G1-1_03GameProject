using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public Material targetMaterial;  // ������ ��Ƽ����
    public string propertyName = "_ShadowLight_Intensity_1"; // �ִϸ��̼��� �Ӽ� �̸�
    public float maxIntensity = 4f;  // �ִ� ��
    public float speed = 1f;         // ��¦�̴� �ӵ� (1�̸� �� 6.28�� �ֱ�)

    private void Update()
    {
        if (targetMaterial == null) return;

        // 0 -> 4 -> 0 ���� ���� (sin � ���)
        float intensity = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f; // 0 ~ 1
        intensity *= maxIntensity; // 0 ~ 4
        targetMaterial.SetFloat(propertyName, intensity);
    }
}
