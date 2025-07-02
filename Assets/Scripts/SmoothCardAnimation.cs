using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SmoothCardAnimation : MonoBehaviour
{
    [Header("애니메이션 설정")]
    private float moveSpeed = 0.3f;
    private float swapSpeed = 0.15f;  // 스왑할 때 더 빠르게!
    public Ease moveEase = Ease.OutBack;
    public Ease swapEase = Ease.OutQuint;  // 스왑할 때 다른 이징

    private Vector3 targetPosition;
    private bool isAnimating = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    // 일반 이동 (기본 속도)
    public void MoveToPosition(Vector3 newPosition)
    {
        MoveToPosition(newPosition, moveSpeed, moveEase);
    }

    // 스왑 이동 (빠른 속도)
    public void SwapToPosition(Vector3 newPosition)
    {
        MoveToPosition(newPosition, swapSpeed, swapEase);
    }

    // 커스텀 속도로 이동
    public void MoveToPosition(Vector3 newPosition, float customSpeed, Ease customEase)
    {
        if (Vector3.Distance(transform.position, newPosition) < 0.1f) return;

        targetPosition = newPosition;

        // 기존 애니메이션 중단
        transform.DOKill();

        isAnimating = true;

        transform.DOMove(targetPosition, customSpeed)
                .SetEase(customEase)
                .OnComplete(() => {
                    isAnimating = false;
                });
    }

    // 현재 애니메이션 중인지 확인
    public bool IsAnimating()
    {
        return isAnimating;
    }

    // 즉시 위치 설정 (애니메이션 없이)
    public void SetPositionInstant(Vector3 newPosition)
    {
        transform.DOKill();
        transform.position = newPosition;
        targetPosition = newPosition;
        isAnimating = false;
    }
}