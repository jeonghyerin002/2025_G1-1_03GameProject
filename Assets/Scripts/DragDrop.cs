using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DragDrop : MonoBehaviour
{
    public bool isDragging = false;
    public Vector3 startPosition;
    public Transform startParent;

    private GameManager gameManager;
    private bool isReturning = false; // 복귀 중인지 확인하는 변수

    void Start()
    {
        startPosition = transform.position;
        startParent = transform.parent;
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (isDragging && !isReturning) // 복귀 중이 아닐 때만 드래그
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }
    }

    private void OnMouseDown()
    {
        // 복귀 중이거나 효과가 진행 중이면 드래그 불가
        if (isReturning || (gameManager != null && !gameManager.CanInteract()))
        {
            return;
        }

        SoundManager.Instance.PlayCardPlace();
        isDragging = true;
        startPosition = transform.position;
        startParent = transform.parent;
        GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    private void OnMouseUp()
    {
        if (isReturning || !isDragging) // 복귀 중이면 무시
        {
            return;
        }

        SoundManager.Instance.PlayCardPlace();

        if (gameManager != null && !gameManager.CanInteract())
        {
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            ReturnToOriginalPosition();
            return;
        }

        isDragging = false;
        GetComponent<SpriteRenderer>().sortingOrder = 1;

        if (gameManager == null)
        {
            ReturnToOriginalPosition();
            return;
        }

        bool wasInMergeArea = startParent == gameManager.mergeArea;

        if (IsOverArea(gameManager.handArea))
        {
            if (wasInMergeArea)
            {
                // 머지 영역에서 손패로 이동
                for (int i = 0; i < gameManager.mergeCount; i++)
                {
                    if (gameManager.mergeCards[i] == gameObject)
                    {
                        for (int j = i; j < gameManager.mergeCount - 1; j++)
                        {
                            gameManager.mergeCards[j] = gameManager.mergeCards[j + 1];
                        }
                        gameManager.mergeCards[gameManager.mergeCount - 1] = null;
                        gameManager.mergeCount--;

                        transform.SetParent(gameManager.handArea);
                        gameManager.handCards[gameManager.handCount] = gameObject;
                        gameManager.handCount++;

                        gameManager.ArrangeHand();
                        gameManager.ArrangeMerge();
                        break;
                    }
                }
            }
            else
            {
                gameManager.ArrangeHand();
            }
        }
        else if (IsOverArea(gameManager.mergeArea))
        {
            if (wasInMergeArea)
            {
                gameManager.ArrangeMerge();
                return;
            }

            if (gameManager.mergeCount >= gameManager.maxMergeSize)
            {
                ReturnToOriginalPosition();
            }
            else
            {
                gameManager.MoveCardToMerge(gameObject);
            }
        }
        else
        {
            ReturnToOriginalPosition();
        }

        if (wasInMergeArea && gameManager.mergeButton != null)
        {
            bool canMerge = gameManager.CanInteract() &&
                           (gameManager.mergeCount == 2 || gameManager.mergeCount == 3);
            gameManager.mergeButton.interactable = canMerge;
        }
    }

    void ReturnToOriginalPosition()
    {
        if (isReturning) return; // 이미 복귀 중이면 무시

        isReturning = true;
        isDragging = false;

        // DOTween 애니메이션으로 부드럽게 복귀
        transform.DOMove(startPosition, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => {
            transform.SetParent(startParent);
            isReturning = false; // 복귀 완료

            if (gameManager != null)
            {
                if (startParent == gameManager.handArea)
                {
                    gameManager.ArrangeHand();
                }
                if (startParent == gameManager.mergeArea)
                {
                    gameManager.ArrangeMerge();
                }
            }
        });
    }

    bool IsOverArea(Transform area)
    {
        if (area == null) return false;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.transform == area)
            {
                return true;
            }
        }
        return false;
    }

    public void ForceStopDrag()
    {
        if (isDragging && !isReturning)
        {
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            ReturnToOriginalPosition();
        }
    }
}