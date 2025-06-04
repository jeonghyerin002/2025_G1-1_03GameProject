using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite[] cardImages;

    public Transform deckArea;
    public Transform handArea;

    public Button drawButton;
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI cardCount;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI notmerge;
    public TextMeshProUGUI maxcard;

    public UIManager uiManager;

    public float cardSpacing = 2.0f;
    public int maxHandSize = 6;

    public GameObject[] deckCards;
    public int deckCount;

    public GameObject[] handCards;
    public int handCount;

    public float chance = 0f;

    public int stage;
    public int score;

    public int[] prefedinedDeck = new int[]
    {
        1,1,1,1,     // A ī��
        2,2,2,2,
        3,3,3,3,
        4,4,4,4,
        5,5,5,5,
        6,6,6,6,
        7,7,7,7,
        8,8,8,8,
        9,9,9,9,
        10,10,10,10,
        11,11,11,11,    //J ī��
        12,12,12,12,    //Q ī��
        13,13,13,13     //K ī�� ��� ī�尡 ���� 4��

    };

    public Transform mergeArea;
    public Button mergeButton;
    public int maxMergeSize = 4;

    public Button DeleteButton;

    public GameObject[] mergeCards;
    public int mergeCount;

    public int gameRound;

    public RoundSO[] roundSOs;

    void Start()
    {
        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];
        mergeCards = new GameObject[maxMergeSize];

        InitializeDeck();
        ShuffleDeck();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowScore(0);
        }
        else
        {
            Debug.LogError("UIManager.Instance�� null�Դϴ�!");
        }

        if (drawButton != null)
        {
            drawButton.onClick.AddListener(OnDrawButtonClicked);
        }

        if (mergeButton != null)
        {
            mergeButton.onClick.AddListener(OnMergeButtonClicked);
            mergeButton.interactable = false;
        }

        if(DeleteButton != null)
        {
            DeleteButton.onClick.AddListener(DeleteMergeCards);  
        }

    }

    void Update()
    {

    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deckCount - 1; i++)
        {
            int j = Random.Range(i, deckCount);



            GameObject temp = deckCards[i];
            deckCards[i] = deckCards[j];
            deckCards[j] = temp;
        }
    }

    void InitializeDeck()
    {
        deckCount = prefedinedDeck.Length;

        for (int i = 0; i < prefedinedDeck.Length; i++)
        {
            int value = prefedinedDeck[i];

            int imageIndex = value - 1;
            if (imageIndex >= cardImages.Length || imageIndex < 0)
            {
                imageIndex = 0;
            }

            GameObject newCardObj = Instantiate(cardPrefab, deckArea.position, Quaternion.identity);
            newCardObj.transform.SetParent(deckArea);
            newCardObj.SetActive(false);

            Card cardComp = newCardObj.GetComponent<Card>();
            if (cardComp != null)
            {
                cardComp.InitCard(value, cardImages[imageIndex]);
            }
            deckCards[i] = newCardObj;


        }
    }

    public void ArrangeHand()
    {
        if (handCount == 0)
            return;

        float startX = -(handCount - 1) * cardSpacing / 2;

        for (int i = 0; i < handCount; i++)
        {
            Vector3 newPos = handArea.position + new Vector3(startX + i * cardSpacing, 0, -0.05f);
            handCards[i].transform.position = newPos;
        }
    }

    public void ArrangeMerge()
    {
        if (mergeCount == 0)
            return;

        float startX = -(mergeCount - 1) * cardSpacing / 2;

        for (int i = 0; i < mergeCount; i++)
        {
            Vector3 newPos = mergeArea.position + new Vector3(startX + i * cardSpacing, 0, -0.05f);
            mergeCards[i].transform.position = newPos;
        }
    }


    public void OnDrawButtonClicked()
    {
        DrawCardToHand();
        Debug.Log("��ư�� ������ �־��."); //��ư Ŭ�� Ȯ��
    }

    public void DrawCardToHand()
    {
        if (handCount + mergeCount >= maxHandSize)
        {
            cardCount.text = "�տ� ī�尡 ���� á��!";
            cardCount.gameObject.SetActive(true);
            Invoke("cardCountTime", 0.5f);
            return;

        }
        else
        {
            cardCount.gameObject.SetActive(false);
        }
        if (deckCount <= 0)
        {
            cardCount.text = "�� �̻� ī�带 ���� �� ���� �� ����";
            cardCount.gameObject.SetActive(true);
            return;
        }
        
        
            GameObject drawnCard = deckCards[0];

        for (int i = 0; i < deckCount - 1; i++)
        {
            deckCards[i] = deckCards[i + 1];
        }
        deckCount--;

        drawnCard.SetActive(true);
        handCards[handCount] = drawnCard;
        handCount++;

        drawnCard.transform.SetParent(handArea);
        //handCards[handCount].transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1.5f);



        ArrangeHand();
    }
    void cardCountTime()
    {
        cardCount.gameObject.SetActive(false);
    }

    void UpdateMergeButtonState()
    {
        if (mergeButton != null)
        {
            mergeButton.interactable = (mergeCount == 2 || mergeCount == 3);
        }
    }

    void DeleteMergeCards()
    {

        if (mergeCount == 0)
        {
            Debug.Log("������ ī�尡 �����ϴ�.");
            return;
        }

        if (mergeCount > 3)
        {
            Debug.Log("�� ���� ������ �� �ִ� �ִ� ī��� 3���Դϴ�.");
            return;
        }

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Destroy(mergeCards[i]);
            }
        }

        for (int i = 0; i < maxMergeSize; i++)
        {
            mergeCards[i] = null;
        }

        mergeCount = 0;
        UpdateMergeButtonState();
        ArrangeMerge();

        Debug.Log("Merge ������ ī����� �����Ǿ����ϴ�.");
    }

    void MergeCards()
    {
        if (mergeCount != 2 && mergeCount != 3 && mergeCount != 4)
        {
            Debug.Log("������ �Ϸ��� ī�尡 2�� �Ǵ� 3�� Ȥ�� 4�� �ʿ��մϴ�.");
            return;
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;
        for (int i = 1; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            if (card == null || card.cardValue != firstCard)
            {
                Debug.Log("���� ������ ī�常 ���� �� �� �ֽ��ϴ�.");
                return;
            }
        }

        int newValue = firstCard + 1;

        int scoreToAdd = newValue * 1;
        score += scoreToAdd;
        Debug.Log($"���� ����! ���� +{scoreToAdd} (���� ����: {score})");


        // �ִ밪 üũ
        if (newValue > cardImages.Length)
        {
            Debug.Log("�ִ� ī�� ���� ���� �߽��ϴ�.");
            return;
        }
        // ���ھ� ����.

        if (newValue > cardImages.Length)
        {
            Debug.Log("�ִ� ī�� ���� ���� �߽��ϴ�.");
            return;
        }

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                mergeCards[i].SetActive(false);
            }
        }

        GameObject newCard = Instantiate(cardPrefab, mergeArea.position, Quaternion.identity);

        Card newCardTemp = newCard.GetComponent<Card>();
        if (newCardTemp != null)
        {
            int imageIndex = newValue - 1;
            newCardTemp.InitCard(newValue, cardImages[imageIndex]);
        }

        for (int i = 0; i < maxMergeSize; i++)
        {
            mergeCards[i] = null;
        }



        mergeCount = 0;

        UpdateMergeButtonState();

        handCards[handCount] = newCard;
        handCount++;
        newCard.transform.SetParent(handArea);

        UIManager.Instance.ShowScore(score);

        ArrangeHand();
        CheckScore();
    }

    public void CheckScore()
    {
        
        if(score >= roundSOs[gameRound - 1].score)
        {
            Debug.Log("�¸�");
        }

    }

    void LuckyChance()
    {

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        switch (firstCard)
        {
            case 1:
                if (mergeCount == 2) chance = 1.0f;
                else if (mergeCount == 3) chance = 0.97f;
                else if (mergeCount == 4) chance = 0.95f;
                break;

            case 2:
                if (mergeCount == 2) chance = 0.92f;
                else if (mergeCount == 3) chance = 0.90f;
                else if (mergeCount == 4) chance = 0.87f;
                break;

            case 3:
                if (mergeCount == 2) chance = 85f;
                else if (mergeCount == 3) chance = 0.82f;
                else if (mergeCount == 4) chance = 0.80f;
                break;

            case 4:
                if (mergeCount == 2) chance = 0.75f;
                else if (mergeCount == 3) chance = 0.72f;
                else if (mergeCount == 4) chance = 0.70f;
                break;

            case 5:
                if (mergeCount == 2) chance = 0.5f;
                else if (mergeCount == 3) chance = 0.45f;
                else if (mergeCount == 4) chance = 0.44f;
                break;

            case 6:
                if (mergeCount == 2) chance = 0.42f;
                else if (mergeCount == 3) chance = 0.40f;
                else if (mergeCount == 4) chance = 0.39f;
                break;

            case 7:
                if (mergeCount == 2) chance = 1.0f;
                else if (mergeCount == 3) chance = 0.8f;
                else if (mergeCount == 4) chance = 0.74f;
                break;

            case 8:
                if (mergeCount == 2) chance = 0.4f;
                else if (mergeCount == 3) chance = 0.38f;
                else if (mergeCount == 4) chance = 0.35f;
                break;

            case 9:
                if (mergeCount == 2) chance = 0.25f;
                else if (mergeCount == 3) chance = 0.23f;
                else if (mergeCount == 4) chance = 0.2f;
                break;

            case 10:
                if (mergeCount == 2) chance = 0.19f;
                else if (mergeCount == 3) chance = 0.19f;
                else if (mergeCount == 4) chance = 0.17f;
                break;

            case 11:
                if (mergeCount == 2) chance = 0.1f;
                else if (mergeCount == 3) chance = 0.08f;
                else if (mergeCount == 4) chance = 0.05f;
                break;

            case 12:
                if (mergeCount == 2) chance = 0.1f;
                else if (mergeCount == 3) chance = 0.08f;
                else if (mergeCount == 4) chance = 0.05f;
                break;

            case 13:
                if (mergeCount == 2) chance = 0f;
                else if (mergeCount == 3) chance = 0f;
                else if (mergeCount == 4) chance = 0f;
                break;

            default:
                Debug.LogWarning($"ī�� Ÿ�� {firstCard}�� ���ǵ��� �ʾҽ��ϴ�.");
                break;


                

        }
    }



    public void MoveCardToMerge(GameObject card)
    {
        if (mergeCount >= maxMergeSize)
        {
            Debug.Log("���� ������ ���� á���ϴ�.!");
            return;
        }
        for (int i = 0; i < handCount; i++)
        {
            if (handCards[i] == card)
            {
                for (int j = i; j < handCount - 1; j++)
                {
                    handCards[j] = handCards[j + 1];
                }
                handCards[handCount - 1] = null;
                handCount--;

                ArrangeHand();
                break;
            }
        }

        mergeCards[mergeCount] = card;
        mergeCount++;

        card.transform.SetParent(mergeArea);
        ArrangeMerge();
        UpdateMergeButtonState();

    }


    public void OnMergeButtonClicked()
    {
        // ī�� ���� ������ ���� Ȯ��
        int value = mergeCards[0].GetComponent<Card>().cardValue;
        for (int i = 1; i < mergeCount; i++)
        {
            if (mergeCards[i].GetComponent<Card>().cardValue != value)
            {
                Debug.Log("���� ������ ī�常 ���� �� �� �ֽ��ϴ�.");
                return;
            }
        }

        if (mergeCount == 0 || mergeCards[0] == null)
            return;

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        if (firstCard == 13)
        {
            maxcard.text = "�� �̻� �ռ� �� �� �����ϴ�.";
            maxcard.gameObject.SetActive(true);
            Invoke("dontmerge", 0.5f);
            Debug.Log("13�� ī���̹Ƿ� �ռ� �Ұ�");
            return;
        }
      

            float GoodChance = Random.value;

        LuckyChance();
      
       
        Debug.Log(chance);
        Debug.Log("�±±±±�" + GoodChance) ;

        if (GoodChance <= chance)
        {
            
            MergeCards();
            Debug.Log("���� �߽��ϴ�!!!!!!!!!!!!!!!");

        }
        else
        {
            Debug.Log("�����߽��ϴ�!!!!!!!!!!!!");
            DeleteMergeCards();
        }
    }

    void dontmerge()
    {
        maxcard.gameObject.SetActive(false);
    }




}





