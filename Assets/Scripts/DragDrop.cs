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

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startParent = transform.parent;

        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }
    }

    private void OnMouseDown()
    {
        SoundManager.Instance.PlayCardPlace();
        // GameManager 상태 확인
        if (gameManager != null && !gameManager.CanInteract())
        {
            Debug.Log("연출 중이므로 드래그를 시작할 수 없습니다.");
            return;
        }

        isDragging = true;

        startPosition = transform.position;
        startParent = transform.parent;

        GetComponent<SpriteRenderer>().sortingOrder = 10;

        Debug.Log("드래그 시작!");
    }

    private void OnMouseUp()
    {
        SoundManager.Instance.PlayCardPlace();
        // GameManager 상태 재확인
        if (gameManager != null && !gameManager.CanInteract())
        {
            Debug.Log("연출 중이므로 드래그를 완료할 수 없습니다. 원래 위치로 복귀합니다.");
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            ReturnToOriginalPosition();
            return;
        }

        isDragging = false;
        GetComponent<SpriteRenderer>().sortingOrder = 1;

        if (gameManager == null)
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
            ReturnToOriginalPosition();
            return;
        }

        bool wasInMergeArea = startParent == gameManager.mergeArea;

        if (IsOverArea(gameManager.handArea))
        {
            Debug.Log("손패 영역으로 이동");

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
                // 손패 내에서 이동
                gameManager.ArrangeHand();
            }
        }
        else if (IsOverArea(gameManager.mergeArea))
        {
            // 이미 머지 영역에 있는 카드라면 그냥 정렬만 하고 끝
            if (wasInMergeArea)
            {
                Debug.Log("같은 머지 영역 내에서 이동 - 정렬만 수행");
                gameManager.ArrangeMerge();
                return;
            }

            // 머지 영역이 가득 찼는지 확인 (기존 코드)
            if (gameManager.mergeCount >= gameManager.maxMergeSize)
            {
                Debug.Log("머지 영역이 가득 찼습니다.");
                ReturnToOriginalPosition();
            }
            else
            {
                // GameManager의 MoveCardToMerge 사용 (기존 코드)
                gameManager.MoveCardToMerge(gameObject);
            }
        }
        else
        {
            Debug.Log("유효하지 않은 영역입니다. 원래 위치로 복귀합니다.");
            ReturnToOriginalPosition();
        }

        // 머지 버튼 상태 업데이트 (머지 영역에서 이동한 경우에만)
        if (wasInMergeArea && gameManager.mergeButton != null)
        {
            bool canMerge = gameManager.CanInteract() &&
                           (gameManager.mergeCount == 2 || gameManager.mergeCount == 3);
            gameManager.mergeButton.interactable = canMerge;
        }
    }

    void ReturnToOriginalPosition()
    {
        // 애니메이션으로 부드럽게 복귀
        transform.DOMove(startPosition, 0.3f).SetEase(Ease.OutQuad).OnComplete(() => {
            transform.SetParent(startParent);

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

        Debug.Log("원래 위치로 복귀했습니다.");
    }

    bool IsOverArea(Transform area)
    {
        if (area == null)
        {
            return false;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.transform == area)
            {
                Debug.Log(area.name + " 영역 감지됨");
                return true;
            }
        }

        return false;
    }

    // 연출 중에 드래그를 강제로 중단하는 함수 (GameManager에서 호출 가능)
    public void ForceStopDrag()
    {
        if (isDragging)
        {
            Debug.Log("연출 시작으로 인해 드래그가 강제 중단됩니다.");
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            ReturnToOriginalPosition();
        }
    }
}