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

        // �巡�� ���� ī�尡 �ִ��� Ȯ��
        CheckForCardSwap();
    }

    void CheckForCardSwap()
    {
        if (isSwapping) return;

        // ���� �巡�� ���� ī�� ã��
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

        // �巡���� ī��� �ٸ� ī����� ��ġ ��
        for (int i = 0; i < gameManager.handCount; i++)
        {
            if (i == draggedIndex || gameManager.handCards[i] == null) continue;

            GameObject otherCard = gameManager.handCards[i];

            // �巡���� ī�尡 �ٸ� ī�带 ���������� Ȯ��
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

        // ���ʿ��� ���������� �̵�
        if (draggedIndex < otherIndex && draggedX > otherX)
        {
            return true;
        }

        // �����ʿ��� �������� �̵�
        if (draggedIndex > otherIndex && draggedX < otherX)
        {
            return true;
        }

        return false;
    }

    IEnumerator SwapCards(int index1, int index2)
    {
        isSwapping = true;

        // ī�� ��ġ ��ü
        GameObject temp = gameManager.handCards[index1];
        gameManager.handCards[index1] = gameManager.handCards[index2];
        gameManager.handCards[index2] = temp;

        // ���� ���� �ִϸ��̼����� ī�� ���ġ
        gameManager.ArrangeHandForSwap();

        // ���� �ִϸ��̼��� ���� ������ ��ٸ��� (�� ª�� �ð�)
        yield return new WaitForSeconds(0.2f);

        isSwapping = false;
    }
}