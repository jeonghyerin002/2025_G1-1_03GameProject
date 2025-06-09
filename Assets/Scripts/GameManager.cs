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

    // 머지 결과 UI 추가
    public TextMeshProUGUI mergeResultText;
    public GameObject mergeSuccessEffect;
    public GameObject mergeFailEffect;

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

    // 연출 상태 관리
    private bool isPlayingEffect = false;

    // DragDrop에서 호출할 수 있도록 public 함수 추가
    public bool IsPlayingEffect()
    {
        return isPlayingEffect;
    }

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

        if (DeleteButton != null)
        {
            DeleteButton.onClick.AddListener(() => {
                Debug.Log($"삭제 버튼 클릭! 현재 mergeCount: {mergeCount}");

                if (mergeCount > 0)
                {
                    Debug.Log("삭제 연출 시작!");
                    StartCoroutine(ShowDeleteEffect());
                    // DelayedDeleteMergeCards() 제거! ShowDeleteEffect에서 자체적으로 처리
                }
                else
                {
                    Debug.Log("삭제할 카드가 없습니다. 머지 영역에 카드를 먼저 놓아주세요!");

                    // 삭제할 카드가 없을 때도 약간의 피드백 제공
                    if (mergeResultText != null)
                    {
                        mergeResultText.text = "삭제할 카드가 없어요!";
                        mergeResultText.color = Color.yellow;
                        mergeResultText.gameObject.SetActive(true);

                        mergeResultText.transform.DOShakePosition(0.5f, strength: 10f, vibrato: 10);

                        mergeResultText.DOFade(0f, 1f).SetDelay(0.5f).OnComplete(() => {
                            mergeResultText.gameObject.SetActive(false);
                            mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
                        });
                    }
                }
            });
        }

        // 머지 결과 텍스트 초기화
        if (mergeResultText != null)
        {
            mergeResultText.gameObject.SetActive(false);
        }
    }

    // 머지 성공 쉐이더 효과 적용
    void ApplyMergeSuccessEffects()
    {
        Debug.Log($"성공 효과 적용 시작! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            Debug.Log($"성공 카드 {i} 확인 중...");

            if (mergeCards[i] != null)
            {
                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    Debug.Log($"성공 카드 {i}에 효과 적용!");
                    cardComponent.PlayMergeSuccessEffect();
                }
                else
                {
                    Debug.LogError($"성공 카드 {i}에 Card 컴포넌트가 없습니다!");

                    // Card 컴포넌트가 없어도 기본 효과라도 보여주기
                    SpriteRenderer sr = mergeCards[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.DOColor(Color.green, 0.2f).SetLoops(4, LoopType.Yoyo);
                        mergeCards[i].transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360);
                    }
                }
            }
            else
            {
                Debug.LogError($"mergeCards[{i}]가 null입니다!");
            }
        }
    }

    // 머지 실패 쉐이더 효과 적용
    void ApplyMergeFailureEffects()
    {
        Debug.Log($"실패 효과 적용 시작! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    Debug.Log($"실패 카드 {i}에 효과 적용!");
                    cardComponent.PlayMergeFailureEffect();
                }
            }
        }
    }

    // 머지 성공 연출 (쉐이더 효과 제거, 기본 UI만)
    IEnumerator ShowMergeSuccess(int newCardValue, int scoreGained)
    {
        // 성공 텍스트 표시
        if (mergeResultText != null)
        {
            mergeResultText.text = $"성공! +{scoreGained}점";
            mergeResultText.color = Color.green;
            mergeResultText.gameObject.SetActive(true);

            // 텍스트 애니메이션
            mergeResultText.transform.localScale = Vector3.zero;
            mergeResultText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.3f);

            mergeResultText.transform.DOScale(1f, 0.2f);

            yield return new WaitForSeconds(1f);

            // 페이드 아웃
            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // 성공 이펙트 활성화
        if (mergeSuccessEffect != null)
        {
            mergeSuccessEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeSuccessEffect.SetActive(false);
        }
    }

    // 머지 실패 연출 (쉐이더 효과 제거, 기본 UI만)
    IEnumerator ShowMergeFailure()
    {
        // 실패 텍스트 표시
        if (mergeResultText != null)
        {
            mergeResultText.text = "실패!";
            mergeResultText.color = Color.red;
            mergeResultText.gameObject.SetActive(true);

            // 흔들기 애니메이션
            mergeResultText.transform.DOShakePosition(1f, strength: 30f, vibrato: 20);

            yield return new WaitForSeconds(1.5f);

            // 페이드 아웃
            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // 실패 이펙트 활성화
        if (mergeFailEffect != null)
        {
            mergeFailEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeFailEffect.SetActive(false);
        }
    }

    // 삭제 연출
    IEnumerator ShowDeleteEffect()
    {
        // 삭제 텍스트 표시
        if (mergeResultText != null)
        {
            mergeResultText.text = "카드 삭제!";
            mergeResultText.color = new Color(1f, 0.5f, 0f); // 주황색
            mergeResultText.gameObject.SetActive(true);

            // 펄스 애니메이션
            mergeResultText.transform.localScale = Vector3.one;
            mergeResultText.transform.DOScale(1.3f, 0.3f).SetLoops(3, LoopType.Yoyo);

            yield return new WaitForSeconds(1f);

            // 페이드 아웃
            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // 카드들에 삭제 쉐이더 효과 적용 (mergeCount가 0이 되기 전에!)
        Debug.Log($"삭제 효과 적용 시작! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Debug.Log($"카드 {i} 확인 중...");

                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    Debug.Log($"카드 {i}에 Card 컴포넌트 발견! 삭제 효과 적용 중...");

                    // 각 카드마다 약간의 딜레이
                    float delay = i * 0.1f;
                    StartCoroutine(DelayedDeleteEffect(cardComponent, delay));
                }
                else
                {
                    Debug.LogError($"카드 {i}에 Card 컴포넌트가 없습니다!");
                }
            }
            else
            {
                Debug.LogError($"mergeCards[{i}]가 null입니다!");
            }
        }

        // 모든 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(0.5f + (mergeCount * 0.1f) + 1f); // 여유시간 추가

        // 이제 실제로 카드들을 삭제
        Debug.Log("ShowDeleteEffect에서 카드 삭제 (DelayedDeleteMergeCards에서도 처리됨)");
        // DeleteMergeCards(); 제거 - DelayedDeleteMergeCards에서 처리

        // 버튼 복원은 DelayedDeleteMergeCards에서 처리
        // RestoreButtonsAfterEffect(); 제거
    }

    // 지연된 삭제 효과
    IEnumerator DelayedDeleteEffect(Card card, float delay)
    {
        Debug.Log($"DelayedDeleteEffect 시작! delay: {delay}");
        yield return new WaitForSeconds(delay);

        if (card != null)
        {
            Debug.Log($"Card.PlayDeleteEffect() 호출!");
            card.PlayDeleteEffect();
        }
        else
        {
            Debug.LogError("Card가 null입니다!");
        }
    }

    public void OnMergeButtonClicked()
    {
        Debug.Log($"머지 버튼 클릭됨! isPlayingEffect: {isPlayingEffect}");

        // 연출 중이면 무시
        if (isPlayingEffect)
        {
            Debug.Log("연출 중이므로 머지 버튼 무시됨");
            return;
        }

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

        // 연출 시작
        Debug.Log("연출 시작! 버튼들 비활성화");
        isPlayingEffect = true;
        SetButtonsInteractable(false);

        float GoodChance = Random.value;
        LuckyChance();

        Debug.Log(chance);
        Debug.Log("랜덤값: " + GoodChance);

        if (GoodChance <= chance)
        {
            // 성공!
            int newValue = firstCard + 1;
            int scoreToAdd = newValue * 1;

            // 쉐이더 효과 먼저 적용
            ApplyMergeSuccessEffects();

            StartCoroutine(ShowMergeSuccess(newValue, scoreToAdd));

            // 잠시 후 실제 머지 처리 (원래대로)
            StartCoroutine(DelayedMergeCards());

            Debug.Log("성공 했습니다!!!!!!!!!!!!!!");
        }
        else
        {
            // 실패!

            // 쉐이더 효과 먼저 적용
            ApplyMergeFailureEffects();

            Debug.Log("실패했습니다!!!!!!!!!!!!");
            StartCoroutine(ShowMergeFailure());

            // 잠시 후 카드 삭제 (원래대로)
            StartCoroutine(DelayedDeleteMergeCards());
        }
    }

    // 지연된 머지 처리 (연출 후) - 원래대로 복원
    IEnumerator DelayedMergeCards()
    {
        Debug.Log("DelayedMergeCards 시작");
        yield return new WaitForSeconds(0.5f); // 연출 시간 대기

        Debug.Log("실제 MergeCards 호출");
        MergeCards();

        // 연출 종료 후 버튼 복원
        Debug.Log("머지 완료 - 버튼 복원");
        RestoreButtonsAfterEffect();
    }

    // 지연된 카드 삭제 (연출 후) - 원래대로 복원  
    IEnumerator DelayedDeleteMergeCards()
    {
        Debug.Log("DelayedDeleteMergeCards 시작");
        yield return new WaitForSeconds(1.2f); // 연출 시간 대기

        Debug.Log("실제 DeleteMergeCards 호출");
        DeleteMergeCards();

        // 연출 종료 후 버튼 복원
        Debug.Log("삭제 완료 - 버튼 복원");
        RestoreButtonsAfterEffect();
    }

    // 버튼 상태 관리 함수
    void SetButtonsInteractable(bool interactable)
    {
        if (drawButton != null)
            drawButton.interactable = interactable;

        if (mergeButton != null)
            mergeButton.interactable = interactable;

        if (DeleteButton != null)
            DeleteButton.interactable = interactable;

        Debug.Log($"버튼들 상태 변경: {(interactable ? "활성화" : "비활성화")}");
    }

    // 연출 종료 후 버튼 복원
    void RestoreButtonsAfterEffect()
    {
        isPlayingEffect = false;

        // 머지 버튼은 조건에 따라 활성화
        UpdateMergeButtonState();

        // 뽑기, 삭제 버튼은 활성화
        if (drawButton != null)
            drawButton.interactable = true;

        if (DeleteButton != null)
            DeleteButton.interactable = true;

        Debug.Log("연출 종료 - 버튼들 복원됨");
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
        Debug.Log($"뽑기 버튼 클릭됨! isPlayingEffect: {isPlayingEffect}");

        // 연출 중이면 무시
        if (isPlayingEffect)
        {
            Debug.Log("연출 중이므로 뽑기 버튼 무시됨");
            return;
        }

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
            cardCount.text = "덱 이상 카드를 뽑을 수 없을 거 같아";
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
        Debug.Log($"DeleteMergeCards 호출됨! mergeCount: {mergeCount}");

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
        }

        int newValue = firstCard + 1;
        int scoreToAdd = newValue * 1;
        score += scoreToAdd;
        Debug.Log($"머지 성공! 점수 +{scoreToAdd} (현재 점수: {score})");

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

        // 새 카드의 크기를 원본 프리팹과 동일하게 설정
        newCard.transform.localScale = cardPrefab.transform.localScale;

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
        if (score >= roundSOs[gameRound - 1].score)
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

    void dontmerge()
    {
        maxcard.gameObject.SetActive(false);
    }
}