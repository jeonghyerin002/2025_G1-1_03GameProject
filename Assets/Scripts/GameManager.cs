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
        1,1,1,1,     // A 카드
        2,2,2,2,
        3,3,3,3,
        4,4,4,4,
        5,5,5,5,
        6,6,6,6,
        7,7,7,7,
        8,8,8,8,
        9,9,9,9,
        10,10,10,10,
        11,11,11,11,    //J 카드
        12,12,12,12,    //Q 카드
        13,13,13,13     //K 카드 모든 카드가 각각 4장

    };

    void Start()
    {
        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];

        InitializeDeck();
        ShuffleDeck();
        
        if (drawButton != null)
        {
            drawButton.onClick.AddListener(OnDrawButtonClicked);
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

    public void OnDrawButtonClicked()
    {
        DrawCardToHand();
        Debug.Log("버튼이 눌리고 있어요."); //버튼 클릭 확인
    }

    public void DrawCardToHand()
    {
        if (handCount >= maxHandSize)
        {
            cardCount.text = "손에 카드가 가득 찼어!";
            cardCount.gameObject.SetActive(true);
            return;
            
        }
        else
        {
            cardCount.gameObject.SetActive(false);
        }
        if (deckCount <= 0)
        {
            cardCount.text = "더 이상 카드를 얻을 수 없을 거 같아";
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



}
