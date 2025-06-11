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
        return !isPlayingEffect && !isMergeEffectPlaying && !isDeleteEffectPlaying && !isClearEffectPlaying;
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
    }

    void InitializeStage()
    {
        if (StageManager.Instance != null)
        {
            StageDataSO currentStageData = StageManager.Instance.GetCurrentStageData();

            // 덱을 스테이지별로 설정
            prefedinedDeck = currentStageData.customDeck;

            // UI 업데이트
            if (stageText != null)
                stageText.text = "Stage " + StageManager.Instance.currentStage;

            if (targetScoreText != null)
                targetScoreText.text = "목표: " + currentStageData.targetScore + "점";

            Debug.Log($"스테이지 {StageManager.Instance.currentStage} 시작!");
        }

        // 배열 초기화
        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];
        mergeCards = new GameObject[maxMergeSize];

        InitializeDeck();
        ShuffleDeck();
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
            StartCoroutine(ShowDeleteEffect());
        }
        else
        {
            Debug.Log("삭제할 카드가 없습니다. 머지 영역에 카드를 먼저 넣어주세요!");
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
        Debug.Log($"머지 버튼 클릭됨! CanInteract: {CanInteract()}");

        // 상호작용 불가능한 상태면 무시
        if (!CanInteract())
        {
            Debug.Log("연출 중이므로 머지 버튼 무시됨");
            return;
        }

        // 카드 값이 같은지 먼저 확인
        if (mergeCount < 2)
        {
            Debug.Log("머지하려면 최소 2장의 카드가 필요합니다.");
            return;
        }

        int value = mergeCards[0].GetComponent<Card>().cardValue;
        for (int i = 1; i < mergeCount; i++)
        {
            if (mergeCards[i].GetComponent<Card>().cardValue != value)
            {
                Debug.Log("같은 숫자의 카드만 머지 할 수 있습니다.");
                ShowWarningMessage("같은 숫자의 카드만 머지 가능!", Color.yellow);
                return;
            }
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        if (firstCard == 13)
        {
            ShowWarningMessage("더 이상 합성 할 수 없습니다.", Color.red);
            Debug.Log("13번 카드이므로 합성 불가");
            return;
        }

        // 연출 시작
        Debug.Log("연출 시작! 버튼들 비활성화");
        isPlayingEffect = true;
        SetButtonsInteractable(false);

        float GoodChance = Random.value;
        LuckyChance();

        Debug.Log($"확률: {chance}, 랜덤값: {GoodChance}");

        if (GoodChance <= chance)
        {
            // 성공!
            int newValue = firstCard + 1;
            int scoreToAdd = newValue * 1;

            // 셰이더 효과 먼저 적용
            ApplyMergeSuccessEffects();

            StartCoroutine(ShowMergeSuccess(newValue, scoreToAdd));

            // 잠시 후 실제 머지 처리 (수정됨으로)
            StartCoroutine(DelayedMergeCards());

            Debug.Log("성공 했습니다!!!!!!!!!!!!!!");
        }
        else
        {
            // 실패!
            // 셰이더 효과 먼저 적용
            ApplyMergeFailureEffects();

            Debug.Log("실패했습니다!!!!!!!!!!!!");
            StartCoroutine(ShowMergeFailure());

            // 잠시 후 카드 삭제 (수정됨으로)
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
        Debug.Log("DelayedDeleteMergeCards 시작");

        // 머지 효과 완료까지 대기
        yield return new WaitUntil(() => !isMergeEffectPlaying);

        yield return new WaitForSeconds(1.2f); // 연출 시간 대기

        Debug.Log("실제 DeleteMergeCards 호출");
        DeleteMergeCards();

        // 연출 종료 후 버튼 복구
        Debug.Log("삭제 완료 - 버튼 복구");
        RestoreButtonsAfterEffect();
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
            return;
        }

        if (deckCount <= 0)
        {
            ShowWarningMessage("덱 이상 카드를 뽑을 수 없을 거 같아", Color.red);
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

    void UpdateMergeButtonState()
    {
        if (mergeButton != null)
        {
            // 연출 중이 아니고, 카드가 2~3장 있을 때만 활성화
            bool canMerge = CanInteract() && (mergeCount == 2 || mergeCount == 3);
            mergeButton.interactable = canMerge;
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
        if (StageManager.Instance != null)
        {
            StageDataSO currentStageData = StageManager.Instance.GetCurrentStageData();

            if (score >= currentStageData.targetScore)
            {
                Debug.Log("스테이지 클리어!");
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
                if (mergeCount == 2) chance = 0.85f; // 수정: 85f -> 0.85f
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
    }
}