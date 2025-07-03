using UnityEngine;
using DG.Tweening;

public class SimpleCardVisualEffects : MonoBehaviour
{
    [Header("회전 효과")]
    public float rotationAmount = 3f;      // 15f -> 3f로 줄임
    public float rotationSpeed = 5f;       // 20f -> 5f로 줄임

    [Header("호버 효과")]
    public float hoverScale = 1.05f;       // 1.1f -> 1.05f로 줄임
    public float hoverDuration = 0.3f;     // 0.2f -> 0.3f로 늘림 (부드럽게)

    [Header("스왑 효과")]
    public float swapRotation = 10f;       // 30f -> 10f로 줄임
    public float swapDuration = 0.25f;     // 0.15f -> 0.25f로 늘림

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
        // 간단한 자동 회전 효과
        if (!isHovering && cardComponent != null && !cardComponent.IsPlayingEffect())
        {
            ApplyIdleRotation();
        }
    }

    void ApplyIdleRotation()
    {
        float sine = Mathf.Sin(Time.time * 0.3f + transform.GetSiblingIndex()) * 0.3f;  // 시간 속도와 강도 줄임
        float rotation = sine * rotationAmount;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(0, 0, rotation), rotationSpeed * Time.deltaTime);
    }

    public void PlayHoverEffect()
    {
        isHovering = true;
        transform.DOScale(originalScale * hoverScale, hoverDuration).SetEase(Ease.OutQuart);  // OutBack -> OutQuart로 변경
        transform.DOPunchRotation(Vector3.forward * 2f, hoverDuration, 5, 0.5f);  // 5f -> 2f, 마지막 값 추가로 더 부드럽게
    }

    public void StopHoverEffect()
    {
        isHovering = false;
        transform.DOScale(originalScale, hoverDuration).SetEase(Ease.OutQuart);  // OutBack -> OutQuart로 변경
    }

    public void PlaySwapEffect(float direction = 1f)
    {
        transform.DOPunchRotation(Vector3.forward * swapRotation * direction, swapDuration, 3, 0.5f);  // vibrato 5->3, elasticity 추가
    }

    public void PlaySelectEffect()
    {
        transform.DOPunchPosition(Vector3.up * 10f, 0.3f, 5, 0.5f);  // 20f->10f, 시간 늘림, elasticity 추가
        transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 5, 0.5f);  // 0.1f->0.05f, 시간 늘림, elasticity 추가
    }
}