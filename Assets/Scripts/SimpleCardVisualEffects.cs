using UnityEngine;
using DG.Tweening;

public class SimpleCardVisualEffects : MonoBehaviour
{
    [Header("ȸ�� ȿ��")]
    public float rotationAmount = 3f;      // 15f -> 3f�� ����
    public float rotationSpeed = 5f;       // 20f -> 5f�� ����

    [Header("ȣ�� ȿ��")]
    public float hoverScale = 1.05f;       // 1.1f -> 1.05f�� ����
    public float hoverDuration = 0.3f;     // 0.2f -> 0.3f�� �ø� (�ε巴��)

    [Header("���� ȿ��")]
    public float swapRotation = 10f;       // 30f -> 10f�� ����
    public float swapDuration = 0.25f;     // 0.15f -> 0.25f�� �ø�

    private Vector3 originalScale;
    private bool isHovering = false;
    private Card cardComponent;

    void Start()
    {
        originalScale = transform.localScale;
        cardComponent = GetComponent<Card>();
    }

    void Update()
    {
        // ������ �ڵ� ȸ�� ȿ��
        if (!isHovering && cardComponent != null && !cardComponent.IsPlayingEffect())
        {
            ApplyIdleRotation();
        }
    }

    void ApplyIdleRotation()
    {
        float sine = Mathf.Sin(Time.time * 0.3f + transform.GetSiblingIndex()) * 0.3f;  // �ð� �ӵ��� ���� ����
        float rotation = sine * rotationAmount;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(0, 0, rotation), rotationSpeed * Time.deltaTime);
    }

    public void PlayHoverEffect()
    {
        isHovering = true;
        transform.DOScale(originalScale * hoverScale, hoverDuration).SetEase(Ease.OutQuart);  // OutBack -> OutQuart�� ����
        transform.DOPunchRotation(Vector3.forward * 2f, hoverDuration, 5, 0.5f);  // 5f -> 2f, ������ �� �߰��� �� �ε巴��
    }

    public void StopHoverEffect()
    {
        isHovering = false;
        transform.DOScale(originalScale, hoverDuration).SetEase(Ease.OutQuart);  // OutBack -> OutQuart�� ����
    }

    public void PlaySwapEffect(float direction = 1f)
    {
        transform.DOPunchRotation(Vector3.forward * swapRotation * direction, swapDuration, 3, 0.5f);  // vibrato 5->3, elasticity �߰�
    }

    public void PlaySelectEffect()
    {
        transform.DOPunchPosition(Vector3.up * 10f, 0.3f, 5, 0.5f);  // 20f->10f, �ð� �ø�, elasticity �߰�
        transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 5, 0.5f);  // 0.1f->0.05f, �ð� �ø�, elasticity �߰�
    }
}