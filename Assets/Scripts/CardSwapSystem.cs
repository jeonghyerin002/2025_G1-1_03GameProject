using System.Collections;
using UnityEngine;

public class CardSwapSystem : MonoBehaviour
{
    private GameManager gameManager;
    private int draggedCardIndex = -1;
    private bool isSwapping = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager == null || !gameManager.CanInteract()) return;

        // 드래그 중인 카드가 있는지 확인
        CheckForCardSwap();
    }

    void CheckForCardSwap()
    {
        if (isSwapping) return;

        // 현재 드래그 중인 카드 찾기
        GameObject draggedCard = null;
        int draggedIndex = -1;

        for (int i = 0; i < gameManager.handCount; i++)
        {
            if (gameManager.handCards[i] != null)
            {
                DragDrop dragDrop = gameManager.handCards[i].GetComponent<DragDrop>();
                if (dragDrop != null && dragDrop.isDragging)
                {
                    draggedCard = gameManager.handCards[i];
                    draggedIndex = i;
                    break;
                }
            }
        }

        if (draggedCard == null) return;

        // 드래그한 카드와 다른 카드들의 위치 비교
        for (int i = 0; i < gameManager.handCount; i++)
        {
            if (i == draggedIndex || gameManager.handCards[i] == null) continue;

            GameObject otherCard = gameManager.handCards[i];

            // 드래그한 카드가 다른 카드를 지나갔는지 확인
            if (ShouldSwapCards(draggedCard, otherCard, draggedIndex, i))
            {
                StartCoroutine(SwapCards(draggedIndex, i));
                break;
            }
        }
    }

    bool ShouldSwapCards(GameObject draggedCard, GameObject otherCard, int draggedIndex, int otherIndex)
    {
        float draggedX = draggedCard.transform.position.x;
        float otherX = otherCard.transform.position.x;

        // 왼쪽에서 오른쪽으로 이동
        if (draggedIndex < otherIndex && draggedX > otherX)
        {
            return true;
        }

        // 오른쪽에서 왼쪽으로 이동
        if (draggedIndex > otherIndex && draggedX < otherX)
        {
            return true;
        }

        return false;
    }

    IEnumerator SwapCards(int index1, int index2)
    {
        isSwapping = true;

        // 카드 위치 교체
        GameObject temp = gameManager.handCards[index1];
        gameManager.handCards[index1] = gameManager.handCards[index2];
        gameManager.handCards[index2] = temp;

        // 빠른 스왑 애니메이션으로 카드 재배치
        gameManager.ArrangeHandForSwap();

        // 스왑 애니메이션이 끝날 때까지 기다리기 (더 짧은 시간)
        yield return new WaitForSeconds(0.2f);

        isSwapping = false;
    }
}