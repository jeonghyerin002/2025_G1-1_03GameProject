using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SmoothCardAnimation : MonoBehaviour
{
    [Header("�ִϸ��̼� ����")]
    private float moveSpeed = 0.3f;
    private float swapSpeed = 0.15f;  // ������ �� �� ������!
    public Ease moveEase = Ease.OutBack;
    public Ease swapEase = Ease.OutQuint;  // ������ �� �ٸ� ��¡

    private Vector3 targetPosition;
    private bool isAnimating = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    // �Ϲ� �̵� (�⺻ �ӵ�)
    public void MoveToPosition(Vector3 newPosition)
    {
        MoveToPosition(newPosition, moveSpeed, moveEase);
    }

    // ���� �̵� (���� �ӵ�)
    public void SwapToPosition(Vector3 newPosition)
    {
        MoveToPosition(newPosition, swapSpeed, swapEase);
    }

    // Ŀ���� �ӵ��� �̵�
    public void MoveToPosition(Vector3 newPosition, float customSpeed, Ease customEase)
    {
        if (Vector3.Distance(transform.position, newPosition) < 0.1f) return;

        targetPosition = newPosition;

        // ���� �ִϸ��̼� �ߴ�
        transform.DOKill();

        isAnimating = true;

        transform.DOMove(targetPosition, customSpeed)
                .SetEase(customEase)
                .OnComplete(() => {
                    isAnimating = false;
                });
    }

    // ���� �ִϸ��̼� ������ Ȯ��
    public bool IsAnimating()
    {
        return isAnimating;
    }

    // ��� ��ġ ���� (�ִϸ��̼� ����)
    public void SetPositionInstant(Vector3 newPosition)
    {
        transform.DOKill();
        transform.position = newPosition;
        targetPosition = newPosition;
        isAnimating = false;
    }
}