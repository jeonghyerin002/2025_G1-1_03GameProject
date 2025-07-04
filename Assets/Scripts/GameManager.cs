using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("SO 데이터 - 새로 추가")]
    public CardInfoSO[] cardDatabase;       // 모든 카드 정보
    public MergeRuleSO[] mergeRules;        // 합성 규칙들
    public bool useSOSystem = true;         // SO 시스템 사용 여부
    public DeckCompositionSO deckComposition; // 덱 구성

    [Header("기존 데이터 - 호환용")]
    public GameObject cardPrefab;
    public Sprite[] cardImages;

    [Header("합성 정보 UI")]
    public MergeInfoUI mergeInfoUI;  

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

    // 스테이지 시스템 UI
    [Header("스테이지 시스템")]
    public TextMeshProUGUI stageText; // 스테이지 표시 UI
    public TextMeshProUGUI targetScoreText; // 목표 점수 표시 UI

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

    // 클리어 연출 상태 변수 추가
    private bool isClearEffectPlaying = false;

    private List<GameObject> cardsToDelete = new List<GameObject>(); // 삭제할 카드들 저장

    [Header("카드 교체 시스템 - 새로 추가")]
    private CardSwapSystem cardSwapSystem;

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
        13,13,13,13,
        14,14,14,14,
        15,15,15,15,
        16,16,16,16//K 카드 모든 카드가 각각 4개
    };

    public Transform mergeArea;
    public Button mergeButton;
    public int maxMergeSize = 4;

    public Button DeleteButton;

    public GameObject[] mergeCards;
    public int mergeCount;

    public int gameRound;

    public RoundSO[] roundSOs;

    // 연출 상태 관리 - 더 세분화
    private bool isPlayingEffect = false;
    private bool isMergeEffectPlaying = false;
    private bool isDeleteEffectPlaying = false;

    private bool isAnyEffectPlaying = false;

    // 드래그 제어를 위한 공개 함수들
    public bool IsPlayingEffect()
    {
        return isPlayingEffect;
    }

    public bool IsMergeEffectPlaying()
    {
        return isMergeEffectPlaying;
    }

    public bool IsDeleteEffectPlaying()
    {
        return isDeleteEffectPlaying;
    }

    // 모든 상호작용이 가능한지 확인
    public bool CanInteract()
    {
        return !isPlayingEffect && !isMergeEffectPlaying && !isDeleteEffectPlaying &&
               !isClearEffectPlaying && !isAnyEffectPlaying;  
    }

    void Start()
    {
        // 스테이지 시스템 먼저 초기화
        InitializeStage();

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
            DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        // 머지 결과 텍스트 초기화
        if (mergeResultText != null)
        {
            mergeResultText.gameObject.SetActive(false);
        }

        // 카드 교체 시스템 추가
        cardSwapSystem = gameObject.AddComponent<CardSwapSystem>();
    }

    void InitializeStage()
    {
        // 기존 스테이지 초기화 유지
        if (StageManager.Instance != null)
        {
            StageDataSO currentStageData = StageManager.Instance.GetCurrentStageData();

            // SO 덱이 없으면 기존 방식 사용
            if (useSOSystem && deckComposition != null)
            {
                // SO 덱 사용
                prefedinedDeck = deckComposition.GetDeckAsIntArray();
                Debug.Log($"SO 덱 사용: {deckComposition.deckName}, 총 {prefedinedDeck.Length}장");
            }
            else
            {
                // 기존 방식 또는 스테이지 덱 사용
                prefedinedDeck = currentStageData.customDeck;
                Debug.Log("기존 덱 방식 사용");
            }
        }
        else
        {
            // StageManager 없을 때 SO 덱 체크
            if (useSOSystem && deckComposition != null)
            {
                prefedinedDeck = deckComposition.GetDeckAsIntArray();
                Debug.Log($"SO 덱 사용 (StageManager 없음): {deckComposition.deckName}");
            }
        }

        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];
        mergeCards = new GameObject[maxMergeSize];

        InitializeDeck();

        // SO 덱 설정에 따라 셔플
        if (useSOSystem && deckComposition != null && deckComposition.shuffleOnStart)
        {
            ShuffleDeck();
        }
        else if (!useSOSystem)
        {
            ShuffleDeck(); // 기존 방식은 항상 셔플
        }
    }

    // 삭제 버튼 클릭 핸들러 부분
    void OnDeleteButtonClicked()
    {
        Debug.Log($"삭제 버튼 클릭! 현재 mergeCount: {mergeCount}, CanInteract: {CanInteract()}");

        // 상호작용 불가능한 상태면 무시
        if (!CanInteract())
        {
            Debug.Log("연출 중이므로 삭제 버튼 무시됨");
            return;
        }

        if (mergeCount > 0)
        {
            Debug.Log("삭제 연출 시작!");
            SoundManager.Instance.PlayDiscard();
            StartCoroutine(ShowDeleteEffect());
        }
        else
        {
            Debug.Log("삭제할 카드가 없습니다. 머지 영역에 카드를 먼저 넣어주세요!");
            SoundManager.Instance.PlayFullWarning();
            ShowWarningMessage("삭제할 카드가 없어요!", Color.yellow);
        }
    }

    // 경고 메시지 표시 (공통 함수)
    void ShowWarningMessage(string message, Color color)
    {
        if (mergeResultText != null)
        {
            mergeResultText.text = message;
            mergeResultText.color = color;
            mergeResultText.gameObject.SetActive(true);

            mergeResultText.transform.DOShakePosition(0.5f, strength: 10f, vibrato: 10);
            mergeResultText.DOFade(0f, 1f).SetDelay(0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }
    }

    // 머지 성공 셰이더 효과 적용
    void ApplyMergeSuccessEffects()
    {
        Debug.Log($"성공 효과 적용 시작! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
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
                    // 기본 효과라도 보여주기
                    SpriteRenderer sr = mergeCards[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.DOColor(Color.green, 0.2f).SetLoops(4, LoopType.Yoyo);
                        mergeCards[i].transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360);
                    }
                }
            }
        }
    }

    // 머지 실패 셰이더 효과 적용
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

    IEnumerator ShowMergeSuccess(int newCardValue, int scoreGained)
    {
        isMergeEffectPlaying = true;

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayMergeSuccessEffect(mergeArea.position);
        }

        // 기존 코드 유지
        if (mergeResultText != null)
        {
            mergeResultText.text = $"성공! +{scoreGained}점";
            mergeResultText.color = Color.green;
            mergeResultText.gameObject.SetActive(true);

            mergeResultText.transform.localScale = Vector3.zero;
            mergeResultText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.3f);

            mergeResultText.transform.DOScale(1f, 0.2f);

            yield return new WaitForSeconds(1f);

            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // 기존 성공 이펙트도 유지
        if (mergeSuccessEffect != null)
        {
            mergeSuccessEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeSuccessEffect.SetActive(false);
        }

        isMergeEffectPlaying = false;
    }

    // 기존 ShowMergeFailure 함수에 이펙트만 추가
    IEnumerator ShowMergeFailure()
    {
        isMergeEffectPlaying = true;

        // ★ 이펙트 재생 추가 (합성 영역 위치에서)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayMergeFailEffect(mergeArea.position);
        }

        // 기존 코드 유지
        if (mergeResultText != null)
        {
            mergeResultText.text = "실패!";
            mergeResultText.color = Color.red;
            mergeResultText.gameObject.SetActive(true);

            mergeResultText.transform.DOShakePosition(1f, strength: 30f, vibrato: 20);

            yield return new WaitForSeconds(1.5f);

            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // 기존 실패 이펙트도 유지
        if (mergeFailEffect != null)
        {
            mergeFailEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeFailEffect.SetActive(false);
        }

        isMergeEffectPlaying = false;
    }

    // 기존 ShowDeleteEffect 함수에 이펙트만 추가
    IEnumerator ShowDeleteEffect()
    {
        isDeleteEffectPlaying = true;
        SetButtonsInteractable(false);

        // ★ 이펙트 재생 추가 (합성 영역 위치에서)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayDeleteEffect(mergeArea.position);
        }

        // 기존 코드 유지
        if (mergeResultText != null)
        {
            mergeResultText.text = "카드 삭제!";
            mergeResultText.color = new Color(1f, 0.5f, 0f);
            mergeResultText.gameObject.SetActive(true);

            mergeResultText.transform.localScale = Vector3.one;
            mergeResultText.transform.DOScale(1.3f, 0.3f).SetLoops(3, LoopType.Yoyo);

            yield return new WaitForSeconds(1f);

            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // 기존 카드 삭제 애니메이션도 유지
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    float delay = i * 0.1f;
                    StartCoroutine(DelayedDeleteEffect(cardComponent, delay));
                }
            }
        }

        yield return new WaitForSeconds(0.5f + (mergeCount * 0.1f) + 1.5f);

        DeleteMergeCards();

        isDeleteEffectPlaying = false;
        RestoreButtonsAfterEffect();
    }

    IEnumerator DelayedDeleteEffect(Card card, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (card != null)
        {
            card.PlayDeleteEffect();
        }
    }

    IEnumerator ShowClearEffect()
    {
        isClearEffectPlaying = true;
        SetButtonsInteractable(false);

        // ★ 이펙트 재생 추가 (화면 중앙에서)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayWinEffect(Camera.main.transform.position);
        }

        // 기존 코드 유지
        if (mergeResultText != null)
        {
            mergeResultText.text = "스테이지 클리어!";
            mergeResultText.color = Color.yellow;
            mergeResultText.gameObject.SetActive(true);

            mergeResultText.transform.localScale = Vector3.zero;
            mergeResultText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.5f);
            mergeResultText.transform.DOScale(1f, 0.3f);
            yield return new WaitForSeconds(1.5f);

            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        yield return new WaitForSeconds(0.5f);

        isClearEffectPlaying = false;

        // 기존 스테이지 진행 로직 유지
        if (StageManager.Instance != null)
        {
            StageManager.Instance.currentStage++;
            StageManager.Instance.isGameCleared = false;
            StageManager.Instance.LoadDialogueScene();
        }
    }

    public void OnMergeButtonClicked()
    {
        if (!CanInteract()) return;

        if (mergeCount < 2)
        {
            Debug.Log("최소 2장의 카드가 필요합니다.");
            return;
        }

        // 삭제할 카드들을 미리 저장!
        cardsToDelete.Clear();
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                cardsToDelete.Add(mergeCards[i]);
            }
        }

        // SO 시스템 사용할 때
        if (useSOSystem && mergeRules != null)
        {
            ProcessMergeWithSO();
        }
        else
        {
            // 기존 방식
            ProcessMergeOldWay();
        }
    }

    void ProcessMergeWithSO()
    {
        // 선택된 카드들의 SO 정보 수집
        List<CardInfoSO> selectedCardInfos = new List<CardInfoSO>();

        for (int i = 0; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            if (card != null && card.cardInfo != null)
            {
                selectedCardInfos.Add(card.cardInfo);
            }
        }

        // 합성 규칙 체크
        MergeRuleSO validRule = CheckMergeRules(selectedCardInfos);

        if (validRule == null)
        {
            ShowWarningMessage("합성할 수 없는 조합입니다!", Color.red);
            return;
        }

        // 확률 계산
        float successRate = GetSuccessRate(validRule, mergeCount);
        bool success = Random.value <= successRate;

        if (success)
        {
            // 성공
            score += validRule.scoreReward;
            SoundManager.Instance.PlayMergeSuccess();
            StartCoroutine(ShowMergeSuccess(validRule.newCardValue, validRule.scoreReward));

            // 기존 성공 효과 적용
            ApplyMergeSuccessEffects();
            StartCoroutine(DelayedMergeCards());
        }
        else
        {
            // 실패
            SoundManager.Instance.PlayMergeFail();
            ApplyMergeFailureEffects();
            StartCoroutine(ShowMergeFailure());
            StartCoroutine(DelayedDeleteMergeCards());
        }
    }

    MergeRuleSO CheckMergeRules(List<CardInfoSO> cards)
    {
        foreach (MergeRuleSO rule in mergeRules)
        {
            if (!rule.isActive) continue;

            if (cards.Count < rule.minCards || cards.Count > rule.maxCards)
                continue;

            if (CheckRuleCondition(cards, rule))
                return rule;
        }
        return null;
    }

    bool CheckRuleCondition(List<CardInfoSO> cards, MergeRuleSO rule)
    {
        switch (rule.ruleType)
        {
            case RuleType.SameNumber:
                return CheckSameNumber(cards);

            case RuleType.SameSuit:
                return CheckSameSuit(cards);

            case RuleType.Pair:
                return cards.Count == 2 && CheckSameNumber(cards);

            case RuleType.Triple:
                return cards.Count == 3 && CheckSameNumber(cards);

            default:
                return false;
        }
    }

    bool CheckSameNumber(List<CardInfoSO> cards)
    {
        int firstNumber = cards[0].number;
        foreach (CardInfoSO card in cards)
        {
            if (card.number != firstNumber)
                return false;
        }
        return true;
    }

    bool CheckSameSuit(List<CardInfoSO> cards)
    {
        CardSuit firstSuit = cards[0].suit;
        foreach (CardInfoSO card in cards)
        {
            if (card.suit != firstSuit)
                return false;
        }
        return true;
    }

    float GetSuccessRate(MergeRuleSO rule, int cardCount)
    {
        int index = cardCount - rule.minCards;
        if (index >= 0 && index < rule.successRates.Length)
            return rule.successRates[index];
        return 0f;
    }

    void ProcessMergeOldWay()
    {
        Debug.Log("기존 합성 로직 시작");

        // 기존 합성 로직 유지
        if (mergeCount != 2 && mergeCount != 3 && mergeCount != 4)
        {
            Debug.Log("2-4장의 카드가 필요합니다.");
            return;
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        // 같은 숫자 체크 (등급은 상관없이)
        for (int i = 1; i < mergeCount; i++)
        {
            int currentCard = mergeCards[i].GetComponent<Card>().cardValue;
            if (currentCard != firstCard)
            {
                ShowWarningMessage("같은 숫자의 카드만 합성 가능!", Color.yellow);
                return;
            }
        }

        // 기존 확률 계산
        LuckyChance();
        bool success = Random.value <= chance;

        if (success)
        {
            int newValue = firstCard + 1;
            int scoreToAdd = newValue * 1;

            SoundManager.Instance.PlayMergeSuccess();
            ApplyMergeSuccessEffects();
            StartCoroutine(ShowMergeSuccess(newValue, scoreToAdd));
            StartCoroutine(DelayedMergeCards());
        }
        else
        {
            SoundManager.Instance.PlayMergeFail();
            ApplyMergeFailureEffects();
            StartCoroutine(ShowMergeFailure());
            StartCoroutine(DelayedDeleteMergeCards());
        }
    }



    // 지연된 머지 처리 (연출 후) - 수정됨으로 복구
    IEnumerator DelayedMergeCards()
    {
        Debug.Log("DelayedMergeCards 시작");

        // 머지 효과 완료까지 대기
        yield return new WaitUntil(() => !isMergeEffectPlaying);

        yield return new WaitForSeconds(0.5f); // 추가 여유시간

        Debug.Log("실제 MergeCards 호출");
        MergeCards();

        // 연출 종료 후 버튼 복구
        Debug.Log("머지 완료 - 버튼 복구");
        RestoreButtonsAfterEffect();
    }

    // 지연된 카드 삭제 (연출 후) - 수정됨으로 복구  
    IEnumerator DelayedDeleteMergeCards()
    {
        yield return new WaitForSeconds(1f);
        DeleteStoredCards(); // 저장된 카드들만 삭제
    }

    void DeleteStoredCards()
    {
        Debug.Log($"저장된 카드 {cardsToDelete.Count}개 삭제 시작");

        // 저장된 카드들만 삭제
        foreach (GameObject card in cardsToDelete)
        {
            if (card != null)
            {
                // mergeCards 배열에서도 제거
                for (int i = 0; i < mergeCount; i++)
                {
                    if (mergeCards[i] == card)
                    {
                        mergeCards[i] = null;
                    }
                }

                Destroy(card);
            }
        }

        // 배열 정리
        CompactMergeArray();

        cardsToDelete.Clear();
        Debug.Log("카드 삭제 완료");
    }

    // 배열 정리 메서드
    void CompactMergeArray()
    {
        int writeIndex = 0;

        // null이 아닌 카드들을 앞쪽으로 이동
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                mergeCards[writeIndex] = mergeCards[i];
                writeIndex++;
            }
        }

        // 나머지 슬롯은 null로 초기화
        for (int i = writeIndex; i < mergeCards.Length; i++)
        {
            mergeCards[i] = null;
        }

        mergeCount = writeIndex;
        UpdateMergeButtonState();
        ArrangeMerge();
    }


    // 버튼 상태 관리 함수
    void SetButtonsInteractable(bool interactable)
    {
        if (drawButton != null)
            drawButton.interactable = interactable;

        if (mergeButton != null && interactable)
        {
            // 활성화시에는 조건에 따라 결정
            UpdateMergeButtonState();
        }
        else if (mergeButton != null)
        {
            // 비활성화시에는 무조건 false
            mergeButton.interactable = false;
        }

        if (DeleteButton != null)
            DeleteButton.interactable = interactable;

        Debug.Log($"버튼들 상태 변경: {(interactable ? "활성화" : "비활성화")}");
    }

    // 연출 종료 후 버튼 복구
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

        Debug.Log("연출 종료 - 버튼들 복구됨");
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

            GameObject newCardObj = Instantiate(cardPrefab, deckArea.position, Quaternion.identity);
            newCardObj.transform.SetParent(deckArea);
            newCardObj.SetActive(false);

            Card cardComp = newCardObj.GetComponent<Card>();
            if (cardComp != null)
            {
                if (useSOSystem)
                {
                    // SO 시스템 사용 - 덱에서 직접 CardInfoSO 가져오기
                    CardInfoSO cardInfo = GetCardInfoFromDeck(i);
                    if (cardInfo != null)
                    {
                        cardComp.InitCard(cardInfo);
                    }
                    else
                    {
                        // SO 없으면 데이터베이스에서 찾기
                        CardInfoSO dbCardInfo = GetCardInfoByValue(value);
                        if (dbCardInfo != null)
                        {
                            cardComp.InitCard(dbCardInfo);
                        }
                        else
                        {
                            // 그것도 없으면 기존 방식
                            InitCardOldWay(cardComp, value);
                        }
                    }
                }
                else
                {
                    // 기존 방식
                    InitCardOldWay(cardComp, value);
                }

                // 덱에 있을 때는 기본 등급으로 설정 (드로우 시 랜덤 등급 적용)
                cardComp.SetCardEdition(CardEdition.REGULAR);
            }
            deckCards[i] = newCardObj;
        }

        Debug.Log($"덱 초기화 완료: {deckCount}장");
    }

    // 덱에서 직접 CardInfoSO 가져오기
    CardInfoSO GetCardInfoFromDeck(int deckIndex)
    {
        if (deckComposition == null) return null;

        CardInfoSO[] deckArray = deckComposition.GetDeckAsArray();
        if (deckIndex >= 0 && deckIndex < deckArray.Length)
        {
            return deckArray[deckIndex];
        }

        return null;
    }

    public void ArrangeHand()
    {
        if (handCount == 0) return;

        float startX = -(handCount - 1) * cardSpacing / 2;

        for (int i = 0; i < handCount; i++)
        {
            if (handCards[i] != null && handCards[i].activeInHierarchy)
            {
                Vector3 targetPos = handArea.position + new Vector3(startX + i * cardSpacing, 0, -0.05f);

                DragDrop dragDrop = handCards[i].GetComponent<DragDrop>();
                Card cardComp = handCards[i].GetComponent<Card>();

                if (dragDrop != null && !dragDrop.isDragging &&
                    cardComp != null && !cardComp.IsPlayingEffect())
                {
                    // 부드러운 애니메이션 컴포넌트 확인 및 추가
                    SmoothCardAnimation smoothAnim = handCards[i].GetComponent<SmoothCardAnimation>();
                    if (smoothAnim == null)
                    {
                        smoothAnim = handCards[i].AddComponent<SmoothCardAnimation>();
                    }

                    // 부드럽게 이동
                    smoothAnim.MoveToPosition(targetPos);
                }
            }
        }
    }

    public void ArrangeMerge()
    {
        if (mergeCount == 0) return;

        float startX = -(mergeCount - 1) * cardSpacing / 2;

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null && mergeCards[i].activeInHierarchy)
            {
                Vector3 targetPos = mergeArea.position + new Vector3(startX + i * cardSpacing, 0, -0.05f);

                DragDrop dragDrop = mergeCards[i].GetComponent<DragDrop>();
                Card cardComp = mergeCards[i].GetComponent<Card>();

                if (dragDrop != null && !dragDrop.isDragging &&
                    cardComp != null && !cardComp.IsPlayingEffect())
                {
                    // 부드러운 애니메이션 컴포넌트 확인 및 추가
                    SmoothCardAnimation smoothAnim = mergeCards[i].GetComponent<SmoothCardAnimation>();
                    if (smoothAnim == null)
                    {
                        smoothAnim = mergeCards[i].AddComponent<SmoothCardAnimation>();
                    }

                    // 부드럽게 이동
                    smoothAnim.MoveToPosition(targetPos);
                }
            }
        }
    }
    public void OnDrawButtonClicked()
    {
        SoundManager.Instance.PlayMergeArea();
        Debug.Log($"뽑기 버튼 클릭됨! CanInteract: {CanInteract()}");

        // 상호작용 불가능한 상태면 무시
        if (!CanInteract())
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
            ShowWarningMessage("손에 카드가 가득 참!", Color.yellow);
            SoundManager.Instance.PlayFullWarning();
            return;
        }

        if (deckCount <= 0)
        {
            Debug.Log("게임 종료! 카드가 모두 소진되었습니다.");
            SoundManager.Instance.PlayRoundFail();
            StartCoroutine(ShowGameEndScreen());
            return;
        }

        GameObject drawnCard = deckCards[0];

        for (int i = 0; i < deckCount - 1; i++)
        {
            deckCards[i] = deckCards[i + 1];
        }
        deckCount--;

        drawnCard.SetActive(true);

        // 드로우할 때 랜덤 등급 설정
        Card cardComponent = drawnCard.GetComponent<Card>();
        if (cardComponent != null)
        {
            SetRandomCardEdition(cardComponent);
        }

        handCards[handCount] = drawnCard;
        handCount++;

        drawnCard.transform.SetParent(handArea);

        // 새로 뽑은 카드는 즉시 위치 설정 후 부드럽게 배치
        SmoothCardAnimation smoothAnim = drawnCard.GetComponent<SmoothCardAnimation>();
        if (smoothAnim == null)
        {
            smoothAnim = drawnCard.AddComponent<SmoothCardAnimation>();
        }

        // 덱 위치에서 시작
        smoothAnim.SetPositionInstant(deckArea.position);

        ArrangeHand();
    }

    // 랜덤 등급 설정 메서드
    void SetRandomCardEdition(Card card)
    {
        float random = Random.Range(0f, 1f);
        CardEdition selectedEdition;

        if (random <= 0.05f)        // 5% 확률 - 전설
            selectedEdition = CardEdition.NEGATIVE;
        else if (random <= 0.2f)    // 15% 확률 - 에픽  
            selectedEdition = CardEdition.POLYCHROME;
        else                        // 80% 확률 - 일반
            selectedEdition = CardEdition.REGULAR;

        // 카드에 등급 설정
        card.SetCardEdition(selectedEdition);

        Debug.Log($"드로우한 카드 등급: {selectedEdition}");
    }



    // 게임 종료 화면을 보여주는 새로운 함수 추가
    IEnumerator ShowGameEndScreen()
    {
        // 모든 버튼 비활성화
        SetButtonsInteractable(false);

        // 게임 종료 이펙트 재생 (추가된 부분)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayGameEndEffect(Camera.main.transform.position);
        }

        // 게임 종료 메시지 표시
        if (mergeResultText != null)
        {
            mergeResultText.text = "게임 종료!\n최종 점수: " + score;
            mergeResultText.color = Color.cyan;
            mergeResultText.gameObject.SetActive(true);

            // 텍스트 애니메이션
            mergeResultText.transform.localScale = Vector3.zero;
            mergeResultText.transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.5f);
            mergeResultText.transform.DOScale(1f, 0.3f);
            yield return new WaitForSeconds(3f); // 3초 동안 메시지 표시
        }

        // 첫 화면으로 돌아가기
        GoToMainMenu();
    }

    void GoToMainMenu()
    {
        Debug.Log("메인 메뉴로 돌아갑니다.");
        SceneManager.LoadScene("Level_0"); //
    }

    void UpdateMergeButtonState()
    {
        if (mergeButton != null)
        {
            // 연출 중이 아니고, 카드가 2~3장 있을 때만 활성화
            bool canMerge = CanInteract() && (mergeCount == 2 || mergeCount == 3);
            mergeButton.interactable = canMerge;
        }
    }

    // 머지 영역 변경될 때마다 호출
    public void UpdateMergeInfo()
    {
        if (mergeInfoUI == null) return;

        if (mergeCount < 2)
        {
            mergeInfoUI.ShowEmptyState();
            return;
        }

        // 합성 가능 여부와 정보 계산
        string ruleName = "합성 불가";
        float successRate = 0f;
        int reward = 0;
        bool canMerge = false;

        if (useSOSystem && mergeRules != null)
        {
            // SO 시스템으로 체크
            var mergeResult = CheckMergeWithSOInfo();
            if (mergeResult != null)
            {
                canMerge = true;
                ruleName = mergeResult.ruleName;
                successRate = mergeResult.successRate;
                reward = mergeResult.reward;
            }
        }
        else
        {
            // 기존 방식으로 체크
            var oldMergeResult = CheckMergeOldWayInfo();
            if (oldMergeResult != null)
            {
                canMerge = true;
                ruleName = oldMergeResult.ruleName;
                successRate = oldMergeResult.successRate;
                reward = oldMergeResult.reward;
            }
        }

        // UI 업데이트
        mergeInfoUI.UpdateMergeInfo(mergeCount, ruleName, successRate, reward, canMerge);
    }

    // SO 시스템용 합성 정보 체크
    MergeInfoResult CheckMergeWithSOInfo()
    {
        List<CardInfoSO> selectedCardInfos = new List<CardInfoSO>();

        for (int i = 0; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            if (card != null && card.cardInfo != null)
            {
                selectedCardInfos.Add(card.cardInfo);
            }
        }

        MergeRuleSO validRule = CheckMergeRules(selectedCardInfos);
        if (validRule != null)
        {
            float successRate = GetSuccessRate(validRule, mergeCount);
            return new MergeInfoResult
            {
                ruleName = validRule.ruleName,
                successRate = successRate,
                reward = validRule.scoreReward
            };
        }

        return null;
    }

    // 기존 방식용 합성 정보 체크
    MergeInfoResult CheckMergeOldWayInfo()
    {
        Debug.Log("기존 방식으로 합성 정보 체크 시작");

        // 같은 숫자 체크 (등급은 상관없이)
        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        for (int i = 1; i < mergeCount; i++)
        {
            int currentCard = mergeCards[i].GetComponent<Card>().cardValue;
            Debug.Log($"카드 비교: {firstCard} vs {currentCard}");

            if (currentCard != firstCard)
            {
                Debug.Log("같은 숫자가 아님 - 합성 불가");
                return null; // 같은 숫자 아님
            }
        }

        // 16번 카드 체크
        if (firstCard == 16)
        {
            Debug.Log("16번 카드는 더 이상 합성 불가");
            return null; // 더 이상 합성 불가
        }

        // 확률 계산 (기존 LuckyChance 로직 사용)
        float chance = CalculateChanceForCard(firstCard, mergeCount);

        Debug.Log($"합성 가능! 카드값: {firstCard}, 개수: {mergeCount}, 확률: {chance * 100}%");

        return new MergeInfoResult
        {
            ruleName = "같은 숫자 합성",
            successRate = chance,
            reward = (firstCard + 1) * 1
        };
    }

    // GameManager.cs의 CalculateChanceForCard 메서드 완성
    float CalculateChanceForCard(int cardValue, int count)
    {
        float chance = 0f;

        switch (cardValue)
        {
            case 1: // 클로버 A
                if (count == 2) chance = 1.0f;
                else if (count == 3) chance = 0.97f;
                else if (count == 4) chance = 0.95f;
                break;

            case 2: // 클로버 J
                if (count == 2) chance = 0.92f;
                else if (count == 3) chance = 0.90f;
                else if (count == 4) chance = 0.90f;
                break;

            case 3: // 클로버 Q
                if (count == 2) chance = 0.89f;
                else if (count == 3) chance = 0.86f;
                else if (count == 4) chance = 0.85f;
                break;

            case 4: // 클로버 K
                if (count == 2) chance = 0.80f;
                else if (count == 3) chance = 0.78f;
                else if (count == 4) chance = 0.75f;
                break;

            case 5: // 다이아 A
                if (count == 2) chance = 0.73f;
                else if (count == 3) chance = 0.70f;
                else if (count == 4) chance = 0.69f;
                break;

            case 6: // 다이아 J
                if (count == 2) chance = 0.67f;
                else if (count == 3) chance = 0.65f;
                else if (count == 4) chance = 0.62f;
                break;

            case 7: // 다이아 Q
                if (count == 2) chance = 0.6f;
                else if (count == 3) chance = 0.58f;
                else if (count == 4) chance = 0.56f;
                break;

            case 8: // 다이아 K
                if (count == 2) chance = 0.5f;
                else if (count == 3) chance = 0.48f;
                else if (count == 4) chance = 0.46f;
                break;

            case 9: // 하트 A
                if (count == 2) chance = 1f;
                else if (count == 3) chance = 0.8f;
                else if (count == 4) chance = 0.74f;
                break;

            case 10: // 하트 J
                if (count == 2) chance = 0.44f;
                else if (count == 3) chance = 0.42f;
                else if (count == 4) chance = 0.4f;
                break;

            case 11: // 하트 Q
                if (count == 2) chance = 0.38f;
                else if (count == 3) chance = 0.36f;
                else if (count == 4) chance = 0.34f;
                break;

            case 12: // 하트 K
                if (count == 2) chance = 0.32f;
                else if (count == 3) chance = 0.3f;
                else if (count == 4) chance = 0.29f;
                break;

            case 13: // 스페이드 A
                if (count == 2) chance = 0.28f;
                else if (count == 3) chance = 0.26f;
                else if (count == 4) chance = 0.24f;
                break;

            case 14: // 스페이드 J
                if (count == 2) chance = 0.2f;
                else if (count == 3) chance = 0.15f;
                else if (count == 4) chance = 0.12f;
                break;

            case 15: // 스페이드 Q
                if (count == 2) chance = 0.12f;
                else if (count == 3) chance = 0.11f;
                else if (count == 4) chance = 0.10f;
                break;

            case 16: // 스페이드 K
                if (count == 2) chance = 0f;
                else if (count == 3) chance = 0f;
                else if (count == 4) chance = 0f;
                break;

            default:
                Debug.LogWarning($"카드 타입 {cardValue}는 정의되지 않았습니다.");
                chance = 0f;
                break;
        }

        Debug.Log($"카드 {cardValue}, {count}장 합성 확률: {chance * 100}%");
        return chance;
    }


    void DeleteMergeCards()
    {
        Debug.Log($"DeleteMergeCards 호출됨! mergeCount: {mergeCount}");

        if (mergeCount == 0)
        {
            Debug.Log("삭제할 카드가 없습니다.");
            SoundManager.Instance.PlayFullWarning();
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

        UpdateMergeInfo();
    }

    void MergeCards()
    {
        if (mergeCount != 2 && mergeCount != 3 && mergeCount != 4)
        {
            Debug.Log("합성을 하려면 카드가 2개 또는 3개 혹은 4개 필요합니다.");
            SoundManager.Instance.PlayFullWarning();
            return;
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        // 등급 보너스 계산
        float totalBonus = 1.0f;
        CardEdition highestEdition = CardEdition.REGULAR;

        for (int i = 0; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            CardEdition cardEdition = card.GetCardEdition();

            // 보너스 적용
            if (CardRaritySystem.Instance != null)
            {
                totalBonus += CardRaritySystem.Instance.GetBonusMultiplier(cardEdition) - 1.0f;
            }

            // 가장 높은 등급 찾기
            if (cardEdition > highestEdition)
            {
                highestEdition = cardEdition;
            }
        }

        int newValue = firstCard + 1;
        int baseScore = newValue * 1;
        int finalScore = Mathf.RoundToInt(baseScore * totalBonus);

        score += finalScore;
        Debug.Log($"합성 성공! 점수 +{finalScore} (기본: {baseScore}, 보너스: {totalBonus:F1}배)");

        // 카드 삭제
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                mergeCards[i].SetActive(false);
            }
        }

        // 새 카드 생성
        GameObject newCard = Instantiate(cardPrefab, mergeArea.position, Quaternion.identity);
        Card newCardComponent = newCard.GetComponent<Card>();

        if (newCardComponent != null)
        {
            int imageIndex = newValue - 1;
            newCardComponent.InitCard(newValue, cardImages[imageIndex]);

            // 등급 업그레이드 시도
            if (CardRaritySystem.Instance != null)
            {
                float upgradeChance = CardRaritySystem.Instance.GetUpgradeChance(highestEdition);
                if (Random.Range(0f, 1f) < upgradeChance)
                {
                    CardEdition upgradedEdition = CardRaritySystem.Instance.UpgradeEdition(highestEdition);
                    newCardComponent.SetCardEdition(upgradedEdition);
                    Debug.Log($"등급 업그레이드! {upgradedEdition}");
                }
                else
                {
                    newCardComponent.SetCardEdition(highestEdition);
                }
            }
        }

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
        if (StageManager.Instance != null)
        {
            StageDataSO currentStageData = StageManager.Instance.GetCurrentStageData();

            if (score >= currentStageData.targetScore)
            {
                Debug.Log("스테이지 클리어!");
                SoundManager.Instance.PlayRoundSuccess();
                StageManager.Instance.isGameCleared = true;

                // 클리어 연출 시작
                StartCoroutine(ShowClearEffect());
            }
        }
    }
   
    IEnumerator WaitAndLoadDialogue()
    {
        yield return new WaitForSeconds(2f); // 2초 대기
        StageManager.Instance.LoadDialogueScene();
    }

    void LuckyChance()
    {
        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        switch (firstCard)
        {
            case 1:              // 클로버  A
                if (mergeCount == 2) chance = 1.0f;
                else if (mergeCount == 3) chance = 0.97f;
                else if (mergeCount == 4) chance = 0.95f;
                break;
                                   //클로버 J
            case 2:
                if (mergeCount == 2) chance = 0.92f;
                else if (mergeCount == 3) chance = 0.90f;
                else if (mergeCount == 4) chance = 0.90f;
                break;

            case 3:                   //클로버 Q
                if (mergeCount == 2) chance = 0.89f; // 수정: 85f -> 0.85f
                else if (mergeCount == 3) chance = 0.86f;
                else if (mergeCount == 4) chance = 0.85f;
                break;

            case 4:                   //클로버 K
                if (mergeCount == 2) chance = 0.80f;
                else if (mergeCount == 3) chance = 0.78f;
                else if (mergeCount == 4) chance = 0.75f;
                break;
                 
            case 5:                   //다이야 A
                if (mergeCount == 2) chance = 0.73f;
                else if (mergeCount == 3) chance = 0.70f;
                else if (mergeCount == 4) chance = 0.69f;
                break;

            case 6:                   //다이야 J
                if (mergeCount == 2) chance = 0.67f;
                else if (mergeCount == 3) chance = 0.65f;
                else if (mergeCount == 4) chance = 0.62f;
                break;

            case 7:                  //다이야 Q
                if (mergeCount == 2) chance = 0.6f;
                else if (mergeCount == 3) chance = 0.58f;
                else if (mergeCount == 4) chance = 0.56f;
                break;

            case 8:                   //다이야 k
                if (mergeCount == 2) chance = 0.5f;
                else if (mergeCount == 3) chance = 0.48f;
                else if (mergeCount == 4) chance = 0.46f;
                break;

            case 9:                 //하트 A
                if (mergeCount == 2) chance = 1f;
                else if (mergeCount == 3) chance = 0.8f;
                else if (mergeCount == 4) chance = 0.74f;
                break;
                 
            case 10:                  //하트 J
                if (mergeCount == 2) chance = 0.44f;
                else if (mergeCount == 3) chance = 0.42f;
                else if (mergeCount == 4) chance = 0.4f;
                break;

            case 11:                     //하트 Q
                if (mergeCount == 2) chance = 0.38f;
                else if (mergeCount == 3) chance = 0.36f;
                else if (mergeCount == 4) chance = 0.34f;
                break;

            case 12:                //하트 K
                if (mergeCount == 2) chance = 0.32f;
                else if (mergeCount == 3) chance = 0.3f;
                else if (mergeCount == 4) chance = 0.29f;
                break;

            case 13:             //스페이드 A
                if (mergeCount == 2) chance = 0.28f;
                else if (mergeCount == 3) chance = 0.26f;
                else if (mergeCount == 4) chance = 0.24f;
                break;

            case 14:             //스페이드 J
                if (mergeCount == 2) chance = 0.2f;
                else if (mergeCount == 3) chance = 0.15f;
                else if (mergeCount == 4) chance = 0.12f;
                break;

            case 15:             //스페이드 Q
                if (mergeCount == 2) chance = 0.12f;
                else if (mergeCount == 3) chance = 0.11f;
                else if (mergeCount == 4) chance = 0.10f;
                break;

            case 16:             //스페이드 K
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
        // 연출 중에는 카드 이동 불가
        if (!CanInteract())
        {
            Debug.Log("연출 중이므로 카드 이동이 제한됩니다.");
            return;
        }

        if (mergeCount >= maxMergeSize)
        {
            Debug.Log("머지 영역이 가득 참습니다!");
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
        UpdateMergeInfo();
    }

    public void SetAnyEffectPlaying(bool playing)
    {
        isAnyEffectPlaying = playing;

        if (playing)
        {
            StopAllCardDragging(); // 이펙트 시작하면 모든 드래그 정지
        }
    }

    // 모든 카드 드래그 정지
    void StopAllCardDragging()
    {
        // 손패 카드 드래그 정지
        for (int i = 0; i < handCount; i++)
        {
            if (handCards[i] != null)
            {
                DragDrop dragDrop = handCards[i].GetComponent<DragDrop>();
                if (dragDrop != null)
                {
                    dragDrop.ForceStopDrag();
                }
            }
        }

        // 머지 카드 드래그 정지
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                DragDrop dragDrop = mergeCards[i].GetComponent<DragDrop>();
                if (dragDrop != null)
                {
                    dragDrop.ForceStopDrag();
                }
            }
        }

    }

    // 스왑할 때 사용하는 빠른 카드 배치
    public void ArrangeHandForSwap()
    {
        if (handCount == 0) return;

        float startX = -(handCount - 1) * cardSpacing / 2;

        for (int i = 0; i < handCount; i++)
        {
            if (handCards[i] != null && handCards[i].activeInHierarchy)
            {
                Vector3 targetPos = handArea.position + new Vector3(startX + i * cardSpacing, 0, -0.05f);

                DragDrop dragDrop = handCards[i].GetComponent<DragDrop>();
                Card cardComp = handCards[i].GetComponent<Card>();

                if (dragDrop != null && !dragDrop.isDragging &&
                    cardComp != null && !cardComp.IsPlayingEffect())
                {
                    // 부드러운 애니메이션 컴포넌트 확인 및 추가
                    SmoothCardAnimation smoothAnim = handCards[i].GetComponent<SmoothCardAnimation>();
                    if (smoothAnim == null)
                    {
                        smoothAnim = handCards[i].AddComponent<SmoothCardAnimation>();
                    }

                    // 스왑용 빠른 이동 사용!
                    smoothAnim.SwapToPosition(targetPos);
                }
            }
        }
    }

    // 카드 위치를 수동으로 바꾸는 함수 (빠른 애니메이션 사용)
    public void SwapHandCards(int index1, int index2)
    {
        if (index1 < 0 || index1 >= handCount || index2 < 0 || index2 >= handCount)
            return;

        if (handCards[index1] == null || handCards[index2] == null)
            return;

        // 카드 위치 교체
        GameObject temp = handCards[index1];
        handCards[index1] = handCards[index2];
        handCards[index2] = temp;

        // 빠른 카드 재배치 사용!
        ArrangeHandForSwap();

        Debug.Log($"카드 {index1}번과 {index2}번 위치 교체!");
    }

    CardInfoSO GetCardInfoByValue(int value)
    {
        if (cardDatabase == null) return null;

        foreach (CardInfoSO card in cardDatabase)
        {
            if (card.gameValue == value)
                return card;
        }
        return null;
    }

    void InitCardOldWay(Card cardComp, int value)
    {
        int imageIndex = value - 1;
        if (imageIndex >= cardImages.Length || imageIndex < 0)
            imageIndex = 0;

        cardComp.InitCard(value, cardImages[imageIndex]);
    }



}

// 결과 데이터 클래스
public class MergeInfoResult
{
    public string ruleName;
    public float successRate;
    public int reward;
}