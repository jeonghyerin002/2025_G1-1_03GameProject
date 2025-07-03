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

        CheckForCardSwap();
    }

    void CheckForCardSwap()
    {
        if (isSwapping) return;

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

        for (int i = 0; i < gameManager.handCount; i++)
        {
            if (i == draggedIndex || gameManager.handCards[i] == null) continue;

            GameObject otherCard = gameManager.handCards[i];

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

        if (draggedIndex < otherIndex && draggedX > otherX)
        {
            return true;
        }

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

        // 새로운 스왑 효과 추가
        Card card1 = gameManager.handCards[index1].GetComponent<Card>();
        Card card2 = gameManager.handCards[index2].GetComponent<Card>();

        if (card1 != null)
        {
            card1.PlaySwapEffect(1f);
        }
        if (card2 != null)
        {
            card2.PlaySwapEffect(-1f);
        }

        gameManager.ArrangeHandForSwap();

        yield return new WaitForSeconds(0.2f);

        isSwapping = false;
    }
}