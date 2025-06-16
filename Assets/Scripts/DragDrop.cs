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
        // GameManager ���� Ȯ��
        if (gameManager != null && !gameManager.CanInteract())
        {
            Debug.Log("���� ���̹Ƿ� �巡�׸� ������ �� �����ϴ�.");
            return;
        }

        isDragging = true;

        startPosition = transform.position;
        startParent = transform.parent;

        GetComponent<SpriteRenderer>().sortingOrder = 10;

        Debug.Log("�巡�� ����!");
    }

    private void OnMouseUp()
    {
        SoundManager.Instance.PlayCardPlace();
        // GameManager ���� ��Ȯ��
        if (gameManager != null && !gameManager.CanInteract())
        {
            Debug.Log("���� ���̹Ƿ� �巡�׸� �Ϸ��� �� �����ϴ�. ���� ��ġ�� �����մϴ�.");
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            ReturnToOriginalPosition();
            return;
        }

        isDragging = false;
        GetComponent<SpriteRenderer>().sortingOrder = 1;

        if (gameManager == null)
        {
            Debug.LogError("GameManager�� ã�� �� �����ϴ�!");
            ReturnToOriginalPosition();
            return;
        }

        bool wasInMergeArea = startParent == gameManager.mergeArea;

        if (IsOverArea(gameManager.handArea))
        {
            Debug.Log("���� �������� �̵�");

            if (wasInMergeArea)
            {
                // ���� �������� ���з� �̵�
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
                // ���� ������ �̵�
                gameManager.ArrangeHand();
            }
        }
        else if (IsOverArea(gameManager.mergeArea))
        {
            // �̹� ���� ������ �ִ� ī���� �׳� ���ĸ� �ϰ� ��
            if (wasInMergeArea)
            {
                Debug.Log("���� ���� ���� ������ �̵� - ���ĸ� ����");
                gameManager.ArrangeMerge();
                return;
            }

            // ���� ������ ���� á���� Ȯ�� (���� �ڵ�)
            if (gameManager.mergeCount >= gameManager.maxMergeSize)
            {
                Debug.Log("���� ������ ���� á���ϴ�.");
                ReturnToOriginalPosition();
            }
            else
            {
                // GameManager�� MoveCardToMerge ��� (���� �ڵ�)
                gameManager.MoveCardToMerge(gameObject);
            }
        }
        else
        {
            Debug.Log("��ȿ���� ���� �����Դϴ�. ���� ��ġ�� �����մϴ�.");
            ReturnToOriginalPosition();
        }

        // ���� ��ư ���� ������Ʈ (���� �������� �̵��� ��쿡��)
        if (wasInMergeArea && gameManager.mergeButton != null)
        {
            bool canMerge = gameManager.CanInteract() &&
                           (gameManager.mergeCount == 2 || gameManager.mergeCount == 3);
            gameManager.mergeButton.interactable = canMerge;
        }
    }

    void ReturnToOriginalPosition()
    {
        // �ִϸ��̼����� �ε巴�� ����
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

        Debug.Log("���� ��ġ�� �����߽��ϴ�.");
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
                Debug.Log(area.name + " ���� ������");
                return true;
            }
        }

        return false;
    }

    // ���� �߿� �巡�׸� ������ �ߴ��ϴ� �Լ� (GameManager���� ȣ�� ����)
    public void ForceStopDrag()
    {
        if (isDragging)
        {
            Debug.Log("���� �������� ���� �巡�װ� ���� �ߴܵ˴ϴ�.");
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            ReturnToOriginalPosition();
        }
    }
}