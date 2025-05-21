using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite[] cardImages;

    public Transform deckArea;
    public Transform handArea;

    public Button drawButton;
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI cardCount;

    public float cardSpacing = 2.0f;
    public int maxHandSize = 6;

    public GameObject[] deckCards;
    public int deckCount;

    public GameObject[] handCards;
    public int handCount;

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

    public GameObject[] mergeCards;
    public int mergeCount;

    void Start()
    {
        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];
        mergeCards = new GameObject[maxMergeSize];

        InitializeDeck();
        ShuffleDeck();
        
        if (drawButton != null)
        {
            drawButton.onClick.AddListener(OnDrawButtonClicked);
        }

        if (mergeButton != null)
        {
            mergeButton.onClick.AddListener(OnMergeButtonClicked);
            mergeButton.interactable = false;
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
            if (cardComp != null )
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

        for(int i = 0; i <deckCount - 1; i++)
        {
            deckCards[i] = deckCards[i + 1];
        }
        deckCount--;

        drawnCard.SetActive(true);
        handCards[handCount] = drawnCard;
        handCount++;

        drawnCard.transform.SetParent(handArea);

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

    void MergeCards()
    {
        if (mergeCount != 2 && mergeCount != 3 && mergeCount !=4)
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

        ArrangeHand();
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


    void OnMergeButtonClicked()
    {
        SimpleMergeChanceWithSwitch Lucky = new SimpleMergeChanceWithSwitch();

        if (Random.value > Lucky.chance)
        {
            MergeCards();
        }
    }




}
public class SimpleMergeChanceWithSwitch
{
    public float chance = 0f;
    public bool TryMerge(string cardType, int count)
    {
        


        switch (cardType)
        {
            case "Card1":
                if (count == 2) chance = 1.0f;
                else if (count == 3) chance = 0.97f;
                else if (count == 4) chance = 0.95f;
                break;

            case "Card2":
                if (count == 2) chance = 0.92f;
                else if (count == 3) chance = 0.90f;
                else if (count == 4) chance = 0.87f;
                break;

            case "Card3":
                if (count == 2) chance = 85f;
                else if (count == 3) chance = 0.82f;
                else if (count == 4) chance = 0.80f;
                break;

            case "Card4":
                if (count == 2) chance = 0.75f;
                else if (count == 3) chance = 0.72f;
                else if (count == 4) chance = 0.70f;
                break;

            case "Card5":
                if (count == 2) chance = 0.5f;
                else if (count == 3) chance = 0.45f;
                else if (count == 4) chance = 0.44f;
                break;

            case "Card6":
                if (count == 2) chance = 0.42f;
                else if (count == 3) chance = 0.40f;
                else if (count == 4) chance = 0.39f;
                break;

            case "Card7":
                if (count == 2) chance = 1.0f;
                else if (count == 3) chance = 0.8f;
                else if (count == 4) chance = 0.74f;
                break;

            case "Card8":
                if (count == 2) chance = 0.4f;
                else if (count == 3) chance = 0.38f;
                else if (count == 4) chance = 0.35f;
                break;

            case "Card9":
                if (count == 2) chance = 0.25f;
                else if (count == 3) chance = 0.23f;
                else if (count == 4) chance = 0.2f;
                break;

            case "Card10":
                if (count == 2) chance = 0.19f;
                else if (count == 3) chance = 0.19f;
                else if (count == 4) chance = 0.17f;
                break;

            case "Card11":
                if (count == 2) chance = 0.1f;
                else if (count == 3) chance = 0.08f;
                else if (count == 4) chance = 0.05f;
                break;

            case "Card12":
                if (count == 2) chance = 0.1f;
                else if (count == 3) chance = 0.08f;
                else if (count == 4) chance = 0.05f;
                break;

            case "Card13":
                if (count == 2) chance = 0.1f;
                else if (count == 3) chance = 0.08f;
                else if (count == 4) chance = 0.05f;
                break;

            default:
                Debug.LogWarning($"ī�� Ÿ�� {cardType}�� ���ǵ��� �ʾҽ��ϴ�.");
                break;
        }

        return UnityEngine.Random.value < chance;

    }
}