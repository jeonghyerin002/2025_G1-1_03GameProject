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
            Debug.LogError("UIManager.Instance가 null입니다!");
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
        Debug.Log("버튼이 눌리고 있어요."); //버튼 클릭 확인
    }

    public void DrawCardToHand()
    {
        if (handCount + mergeCount >= maxHandSize)
        {
            cardCount.text = "손에 카드가 가득 찼어!";
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
            cardCount.text = "더 이상 카드를 얻을 수 없을 거 같아";
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
            Debug.Log("삭제할 카드가 없습니다.");
            return;
        }

        if (mergeCount > 3)
        {
            Debug.Log("한 번에 삭제할 수 있는 최대 카드는 3장입니다.");
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

        Debug.Log("Merge 영역의 카드들이 삭제되었습니다.");
    }

    void MergeCards()
    {
        if (mergeCount != 2 && mergeCount != 3 && mergeCount != 4)
        {
            Debug.Log("머지를 하려면 카드가 2개 또는 3개 혹은 4개 필요합니다.");
            return;
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;
        for (int i = 1; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            if (card == null || card.cardValue != firstCard)
            {
                Debug.Log("같은 숫자의 카드만 머지 할 수 있습니다.");
                return;
            }
        }

        int newValue = firstCard + 1;

        int scoreToAdd = newValue * 1;
        score += scoreToAdd;
        Debug.Log($"머지 성공! 점수 +{scoreToAdd} (현재 점수: {score})");


        // 최대값 체크
        if (newValue > cardImages.Length)
        {
            Debug.Log("최대 카드 값에 도달 했습니다.");
            return;
        }
        // 스코어 적기.

        if (newValue > cardImages.Length)
        {
            Debug.Log("최대 카드 값에 도달 했습니다.");
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
            Debug.Log("승리");
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
                Debug.LogWarning($"카드 타입 {firstCard}는 정의되지 않았습니다.");
                break;


                

        }
    }



    public void MoveCardToMerge(GameObject card)
    {
        if (mergeCount >= maxMergeSize)
        {
            Debug.Log("머지 영역이 가득 찼습니다.!");
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
        // 카드 값이 같은지 먼저 확인
        int value = mergeCards[0].GetComponent<Card>().cardValue;
        for (int i = 1; i < mergeCount; i++)
        {
            if (mergeCards[i].GetComponent<Card>().cardValue != value)
            {
                Debug.Log("같은 숫자의 카드만 머지 할 수 있습니다.");
                return;
            }
        }

        if (mergeCount == 0 || mergeCards[0] == null)
            return;

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        if (firstCard == 13)
        {
            maxcard.text = "더 이상 합성 할 수 없습니다.";
            maxcard.gameObject.SetActive(true);
            Invoke("dontmerge", 0.5f);
            Debug.Log("13번 카드이므로 합성 불가");
            return;
        }
      

            float GoodChance = Random.value;

        LuckyChance();
      
       
        Debug.Log(chance);
        Debug.Log("굿굿굿굿굿" + GoodChance) ;

        if (GoodChance <= chance)
        {
            
            MergeCards();
            Debug.Log("성공 했습니다!!!!!!!!!!!!!!!");

        }
        else
        {
            Debug.Log("실패했습니다!!!!!!!!!!!!");
            DeleteMergeCards();
        }
    }

    void dontmerge()
    {
        maxcard.gameObject.SetActive(false);
    }




}





