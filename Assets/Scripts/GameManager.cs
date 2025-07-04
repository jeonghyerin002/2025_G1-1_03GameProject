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
    [Header("SO ������ - ���� �߰�")]
    public CardInfoSO[] cardDatabase;       // ��� ī�� ����
    public MergeRuleSO[] mergeRules;        // �ռ� ��Ģ��
    public bool useSOSystem = true;         // SO �ý��� ��� ����
    public DeckCompositionSO deckComposition; // �� ����

    [Header("���� ������ - ȣȯ��")]
    public GameObject cardPrefab;
    public Sprite[] cardImages;

    [Header("�ռ� ���� UI")]
    public MergeInfoUI mergeInfoUI;  

    public Transform deckArea;
    public Transform handArea;

    public Button drawButton;
    public TextMeshProUGUI deckCountText;
    public TextMeshProUGUI cardCount;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI notmerge;
    public TextMeshProUGUI maxcard;

    // ���� ��� UI �߰�
    public TextMeshProUGUI mergeResultText;
    public GameObject mergeSuccessEffect;
    public GameObject mergeFailEffect;

    // �������� �ý��� UI
    [Header("�������� �ý���")]
    public TextMeshProUGUI stageText; // �������� ǥ�� UI
    public TextMeshProUGUI targetScoreText; // ��ǥ ���� ǥ�� UI

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

    // Ŭ���� ���� ���� ���� �߰�
    private bool isClearEffectPlaying = false;

    private List<GameObject> cardsToDelete = new List<GameObject>(); // ������ ī��� ����

    [Header("ī�� ��ü �ý��� - ���� �߰�")]
    private CardSwapSystem cardSwapSystem;

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
        13,13,13,13,
        14,14,14,14,
        15,15,15,15,
        16,16,16,16//K ī�� ��� ī�尡 ���� 4��
    };

    public Transform mergeArea;
    public Button mergeButton;
    public int maxMergeSize = 4;

    public Button DeleteButton;

    public GameObject[] mergeCards;
    public int mergeCount;

    public int gameRound;

    public RoundSO[] roundSOs;

    // ���� ���� ���� - �� ����ȭ
    private bool isPlayingEffect = false;
    private bool isMergeEffectPlaying = false;
    private bool isDeleteEffectPlaying = false;

    private bool isAnyEffectPlaying = false;

    // �巡�� ��� ���� ���� �Լ���
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

    // ��� ��ȣ�ۿ��� �������� Ȯ��
    public bool CanInteract()
    {
        return !isPlayingEffect && !isMergeEffectPlaying && !isDeleteEffectPlaying &&
               !isClearEffectPlaying && !isAnyEffectPlaying;  
    }

    void Start()
    {
        // �������� �ý��� ���� �ʱ�ȭ
        InitializeStage();

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

        if (DeleteButton != null)
        {
            DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        // ���� ��� �ؽ�Ʈ �ʱ�ȭ
        if (mergeResultText != null)
        {
            mergeResultText.gameObject.SetActive(false);
        }

        // ī�� ��ü �ý��� �߰�
        cardSwapSystem = gameObject.AddComponent<CardSwapSystem>();
    }

    void InitializeStage()
    {
        // ���� �������� �ʱ�ȭ ����
        if (StageManager.Instance != null)
        {
            StageDataSO currentStageData = StageManager.Instance.GetCurrentStageData();

            // SO ���� ������ ���� ��� ���
            if (useSOSystem && deckComposition != null)
            {
                // SO �� ���
                prefedinedDeck = deckComposition.GetDeckAsIntArray();
                Debug.Log($"SO �� ���: {deckComposition.deckName}, �� {prefedinedDeck.Length}��");
            }
            else
            {
                // ���� ��� �Ǵ� �������� �� ���
                prefedinedDeck = currentStageData.customDeck;
                Debug.Log("���� �� ��� ���");
            }
        }
        else
        {
            // StageManager ���� �� SO �� üũ
            if (useSOSystem && deckComposition != null)
            {
                prefedinedDeck = deckComposition.GetDeckAsIntArray();
                Debug.Log($"SO �� ��� (StageManager ����): {deckComposition.deckName}");
            }
        }

        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];
        mergeCards = new GameObject[maxMergeSize];

        InitializeDeck();

        // SO �� ������ ���� ����
        if (useSOSystem && deckComposition != null && deckComposition.shuffleOnStart)
        {
            ShuffleDeck();
        }
        else if (!useSOSystem)
        {
            ShuffleDeck(); // ���� ����� �׻� ����
        }
    }

    // ���� ��ư Ŭ�� �ڵ鷯 �κ�
    void OnDeleteButtonClicked()
    {
        Debug.Log($"���� ��ư Ŭ��! ���� mergeCount: {mergeCount}, CanInteract: {CanInteract()}");

        // ��ȣ�ۿ� �Ұ����� ���¸� ����
        if (!CanInteract())
        {
            Debug.Log("���� ���̹Ƿ� ���� ��ư ���õ�");
            return;
        }

        if (mergeCount > 0)
        {
            Debug.Log("���� ���� ����!");
            SoundManager.Instance.PlayDiscard();
            StartCoroutine(ShowDeleteEffect());
        }
        else
        {
            Debug.Log("������ ī�尡 �����ϴ�. ���� ������ ī�带 ���� �־��ּ���!");
            SoundManager.Instance.PlayFullWarning();
            ShowWarningMessage("������ ī�尡 �����!", Color.yellow);
        }
    }

    // ��� �޽��� ǥ�� (���� �Լ�)
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

    // ���� ���� ���̴� ȿ�� ����
    void ApplyMergeSuccessEffects()
    {
        Debug.Log($"���� ȿ�� ���� ����! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    Debug.Log($"���� ī�� {i}�� ȿ�� ����!");
                    cardComponent.PlayMergeSuccessEffect();
                }
                else
                {
                    Debug.LogError($"���� ī�� {i}�� Card ������Ʈ�� �����ϴ�!");
                    // �⺻ ȿ���� �����ֱ�
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

    // ���� ���� ���̴� ȿ�� ����
    void ApplyMergeFailureEffects()
    {
        Debug.Log($"���� ȿ�� ���� ����! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    Debug.Log($"���� ī�� {i}�� ȿ�� ����!");
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

        // ���� �ڵ� ����
        if (mergeResultText != null)
        {
            mergeResultText.text = $"����! +{scoreGained}��";
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

        // ���� ���� ����Ʈ�� ����
        if (mergeSuccessEffect != null)
        {
            mergeSuccessEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeSuccessEffect.SetActive(false);
        }

        isMergeEffectPlaying = false;
    }

    // ���� ShowMergeFailure �Լ��� ����Ʈ�� �߰�
    IEnumerator ShowMergeFailure()
    {
        isMergeEffectPlaying = true;

        // �� ����Ʈ ��� �߰� (�ռ� ���� ��ġ����)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayMergeFailEffect(mergeArea.position);
        }

        // ���� �ڵ� ����
        if (mergeResultText != null)
        {
            mergeResultText.text = "����!";
            mergeResultText.color = Color.red;
            mergeResultText.gameObject.SetActive(true);

            mergeResultText.transform.DOShakePosition(1f, strength: 30f, vibrato: 20);

            yield return new WaitForSeconds(1.5f);

            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // ���� ���� ����Ʈ�� ����
        if (mergeFailEffect != null)
        {
            mergeFailEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeFailEffect.SetActive(false);
        }

        isMergeEffectPlaying = false;
    }

    // ���� ShowDeleteEffect �Լ��� ����Ʈ�� �߰�
    IEnumerator ShowDeleteEffect()
    {
        isDeleteEffectPlaying = true;
        SetButtonsInteractable(false);

        // �� ����Ʈ ��� �߰� (�ռ� ���� ��ġ����)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayDeleteEffect(mergeArea.position);
        }

        // ���� �ڵ� ����
        if (mergeResultText != null)
        {
            mergeResultText.text = "ī�� ����!";
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

        // ���� ī�� ���� �ִϸ��̼ǵ� ����
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

        // �� ����Ʈ ��� �߰� (ȭ�� �߾ӿ���)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayWinEffect(Camera.main.transform.position);
        }

        // ���� �ڵ� ����
        if (mergeResultText != null)
        {
            mergeResultText.text = "�������� Ŭ����!";
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

        // ���� �������� ���� ���� ����
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
            Debug.Log("�ּ� 2���� ī�尡 �ʿ��մϴ�.");
            return;
        }

        // ������ ī����� �̸� ����!
        cardsToDelete.Clear();
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                cardsToDelete.Add(mergeCards[i]);
            }
        }

        // SO �ý��� ����� ��
        if (useSOSystem && mergeRules != null)
        {
            ProcessMergeWithSO();
        }
        else
        {
            // ���� ���
            ProcessMergeOldWay();
        }
    }

    void ProcessMergeWithSO()
    {
        // ���õ� ī����� SO ���� ����
        List<CardInfoSO> selectedCardInfos = new List<CardInfoSO>();

        for (int i = 0; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            if (card != null && card.cardInfo != null)
            {
                selectedCardInfos.Add(card.cardInfo);
            }
        }

        // �ռ� ��Ģ üũ
        MergeRuleSO validRule = CheckMergeRules(selectedCardInfos);

        if (validRule == null)
        {
            ShowWarningMessage("�ռ��� �� ���� �����Դϴ�!", Color.red);
            return;
        }

        // Ȯ�� ���
        float successRate = GetSuccessRate(validRule, mergeCount);
        bool success = Random.value <= successRate;

        if (success)
        {
            // ����
            score += validRule.scoreReward;
            SoundManager.Instance.PlayMergeSuccess();
            StartCoroutine(ShowMergeSuccess(validRule.newCardValue, validRule.scoreReward));

            // ���� ���� ȿ�� ����
            ApplyMergeSuccessEffects();
            StartCoroutine(DelayedMergeCards());
        }
        else
        {
            // ����
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
        Debug.Log("���� �ռ� ���� ����");

        // ���� �ռ� ���� ����
        if (mergeCount != 2 && mergeCount != 3 && mergeCount != 4)
        {
            Debug.Log("2-4���� ī�尡 �ʿ��մϴ�.");
            return;
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        // ���� ���� üũ (����� �������)
        for (int i = 1; i < mergeCount; i++)
        {
            int currentCard = mergeCards[i].GetComponent<Card>().cardValue;
            if (currentCard != firstCard)
            {
                ShowWarningMessage("���� ������ ī�常 �ռ� ����!", Color.yellow);
                return;
            }
        }

        // ���� Ȯ�� ���
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



    // ������ ���� ó�� (���� ��) - ���������� ����
    IEnumerator DelayedMergeCards()
    {
        Debug.Log("DelayedMergeCards ����");

        // ���� ȿ�� �Ϸ���� ���
        yield return new WaitUntil(() => !isMergeEffectPlaying);

        yield return new WaitForSeconds(0.5f); // �߰� �����ð�

        Debug.Log("���� MergeCards ȣ��");
        MergeCards();

        // ���� ���� �� ��ư ����
        Debug.Log("���� �Ϸ� - ��ư ����");
        RestoreButtonsAfterEffect();
    }

    // ������ ī�� ���� (���� ��) - ���������� ����  
    IEnumerator DelayedDeleteMergeCards()
    {
        yield return new WaitForSeconds(1f);
        DeleteStoredCards(); // ����� ī��鸸 ����
    }

    void DeleteStoredCards()
    {
        Debug.Log($"����� ī�� {cardsToDelete.Count}�� ���� ����");

        // ����� ī��鸸 ����
        foreach (GameObject card in cardsToDelete)
        {
            if (card != null)
            {
                // mergeCards �迭������ ����
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

        // �迭 ����
        CompactMergeArray();

        cardsToDelete.Clear();
        Debug.Log("ī�� ���� �Ϸ�");
    }

    // �迭 ���� �޼���
    void CompactMergeArray()
    {
        int writeIndex = 0;

        // null�� �ƴ� ī����� �������� �̵�
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                mergeCards[writeIndex] = mergeCards[i];
                writeIndex++;
            }
        }

        // ������ ������ null�� �ʱ�ȭ
        for (int i = writeIndex; i < mergeCards.Length; i++)
        {
            mergeCards[i] = null;
        }

        mergeCount = writeIndex;
        UpdateMergeButtonState();
        ArrangeMerge();
    }


    // ��ư ���� ���� �Լ�
    void SetButtonsInteractable(bool interactable)
    {
        if (drawButton != null)
            drawButton.interactable = interactable;

        if (mergeButton != null && interactable)
        {
            // Ȱ��ȭ�ÿ��� ���ǿ� ���� ����
            UpdateMergeButtonState();
        }
        else if (mergeButton != null)
        {
            // ��Ȱ��ȭ�ÿ��� ������ false
            mergeButton.interactable = false;
        }

        if (DeleteButton != null)
            DeleteButton.interactable = interactable;

        Debug.Log($"��ư�� ���� ����: {(interactable ? "Ȱ��ȭ" : "��Ȱ��ȭ")}");
    }

    // ���� ���� �� ��ư ����
    void RestoreButtonsAfterEffect()
    {
        isPlayingEffect = false;

        // ���� ��ư�� ���ǿ� ���� Ȱ��ȭ
        UpdateMergeButtonState();

        // �̱�, ���� ��ư�� Ȱ��ȭ
        if (drawButton != null)
            drawButton.interactable = true;

        if (DeleteButton != null)
            DeleteButton.interactable = true;

        Debug.Log("���� ���� - ��ư�� ������");
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
                    // SO �ý��� ��� - ������ ���� CardInfoSO ��������
                    CardInfoSO cardInfo = GetCardInfoFromDeck(i);
                    if (cardInfo != null)
                    {
                        cardComp.InitCard(cardInfo);
                    }
                    else
                    {
                        // SO ������ �����ͺ��̽����� ã��
                        CardInfoSO dbCardInfo = GetCardInfoByValue(value);
                        if (dbCardInfo != null)
                        {
                            cardComp.InitCard(dbCardInfo);
                        }
                        else
                        {
                            // �װ͵� ������ ���� ���
                            InitCardOldWay(cardComp, value);
                        }
                    }
                }
                else
                {
                    // ���� ���
                    InitCardOldWay(cardComp, value);
                }

                // ���� ���� ���� �⺻ ������� ���� (��ο� �� ���� ��� ����)
                cardComp.SetCardEdition(CardEdition.REGULAR);
            }
            deckCards[i] = newCardObj;
        }

        Debug.Log($"�� �ʱ�ȭ �Ϸ�: {deckCount}��");
    }

    // ������ ���� CardInfoSO ��������
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
                    // �ε巯�� �ִϸ��̼� ������Ʈ Ȯ�� �� �߰�
                    SmoothCardAnimation smoothAnim = handCards[i].GetComponent<SmoothCardAnimation>();
                    if (smoothAnim == null)
                    {
                        smoothAnim = handCards[i].AddComponent<SmoothCardAnimation>();
                    }

                    // �ε巴�� �̵�
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
                    // �ε巯�� �ִϸ��̼� ������Ʈ Ȯ�� �� �߰�
                    SmoothCardAnimation smoothAnim = mergeCards[i].GetComponent<SmoothCardAnimation>();
                    if (smoothAnim == null)
                    {
                        smoothAnim = mergeCards[i].AddComponent<SmoothCardAnimation>();
                    }

                    // �ε巴�� �̵�
                    smoothAnim.MoveToPosition(targetPos);
                }
            }
        }
    }
    public void OnDrawButtonClicked()
    {
        SoundManager.Instance.PlayMergeArea();
        Debug.Log($"�̱� ��ư Ŭ����! CanInteract: {CanInteract()}");

        // ��ȣ�ۿ� �Ұ����� ���¸� ����
        if (!CanInteract())
        {
            Debug.Log("���� ���̹Ƿ� �̱� ��ư ���õ�");
            return;
        }

        DrawCardToHand();
        Debug.Log("��ư�� ������ �־��."); //��ư Ŭ�� Ȯ��
    }

    public void DrawCardToHand()
    {
        if (handCount + mergeCount >= maxHandSize)
        {
            ShowWarningMessage("�տ� ī�尡 ���� ��!", Color.yellow);
            SoundManager.Instance.PlayFullWarning();
            return;
        }

        if (deckCount <= 0)
        {
            Debug.Log("���� ����! ī�尡 ��� �����Ǿ����ϴ�.");
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

        // ��ο��� �� ���� ��� ����
        Card cardComponent = drawnCard.GetComponent<Card>();
        if (cardComponent != null)
        {
            SetRandomCardEdition(cardComponent);
        }

        handCards[handCount] = drawnCard;
        handCount++;

        drawnCard.transform.SetParent(handArea);

        // ���� ���� ī��� ��� ��ġ ���� �� �ε巴�� ��ġ
        SmoothCardAnimation smoothAnim = drawnCard.GetComponent<SmoothCardAnimation>();
        if (smoothAnim == null)
        {
            smoothAnim = drawnCard.AddComponent<SmoothCardAnimation>();
        }

        // �� ��ġ���� ����
        smoothAnim.SetPositionInstant(deckArea.position);

        ArrangeHand();
    }

    // ���� ��� ���� �޼���
    void SetRandomCardEdition(Card card)
    {
        float random = Random.Range(0f, 1f);
        CardEdition selectedEdition;

        if (random <= 0.05f)        // 5% Ȯ�� - ����
            selectedEdition = CardEdition.NEGATIVE;
        else if (random <= 0.2f)    // 15% Ȯ�� - ����  
            selectedEdition = CardEdition.POLYCHROME;
        else                        // 80% Ȯ�� - �Ϲ�
            selectedEdition = CardEdition.REGULAR;

        // ī�忡 ��� ����
        card.SetCardEdition(selectedEdition);

        Debug.Log($"��ο��� ī�� ���: {selectedEdition}");
    }



    // ���� ���� ȭ���� �����ִ� ���ο� �Լ� �߰�
    IEnumerator ShowGameEndScreen()
    {
        // ��� ��ư ��Ȱ��ȭ
        SetButtonsInteractable(false);

        // ���� ���� ����Ʈ ��� (�߰��� �κ�)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayGameEndEffect(Camera.main.transform.position);
        }

        // ���� ���� �޽��� ǥ��
        if (mergeResultText != null)
        {
            mergeResultText.text = "���� ����!\n���� ����: " + score;
            mergeResultText.color = Color.cyan;
            mergeResultText.gameObject.SetActive(true);

            // �ؽ�Ʈ �ִϸ��̼�
            mergeResultText.transform.localScale = Vector3.zero;
            mergeResultText.transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.5f);
            mergeResultText.transform.DOScale(1f, 0.3f);
            yield return new WaitForSeconds(3f); // 3�� ���� �޽��� ǥ��
        }

        // ù ȭ������ ���ư���
        GoToMainMenu();
    }

    void GoToMainMenu()
    {
        Debug.Log("���� �޴��� ���ư��ϴ�.");
        SceneManager.LoadScene("Level_0"); //
    }

    void UpdateMergeButtonState()
    {
        if (mergeButton != null)
        {
            // ���� ���� �ƴϰ�, ī�尡 2~3�� ���� ���� Ȱ��ȭ
            bool canMerge = CanInteract() && (mergeCount == 2 || mergeCount == 3);
            mergeButton.interactable = canMerge;
        }
    }

    // ���� ���� ����� ������ ȣ��
    public void UpdateMergeInfo()
    {
        if (mergeInfoUI == null) return;

        if (mergeCount < 2)
        {
            mergeInfoUI.ShowEmptyState();
            return;
        }

        // �ռ� ���� ���ο� ���� ���
        string ruleName = "�ռ� �Ұ�";
        float successRate = 0f;
        int reward = 0;
        bool canMerge = false;

        if (useSOSystem && mergeRules != null)
        {
            // SO �ý������� üũ
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
            // ���� ������� üũ
            var oldMergeResult = CheckMergeOldWayInfo();
            if (oldMergeResult != null)
            {
                canMerge = true;
                ruleName = oldMergeResult.ruleName;
                successRate = oldMergeResult.successRate;
                reward = oldMergeResult.reward;
            }
        }

        // UI ������Ʈ
        mergeInfoUI.UpdateMergeInfo(mergeCount, ruleName, successRate, reward, canMerge);
    }

    // SO �ý��ۿ� �ռ� ���� üũ
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

    // ���� ��Ŀ� �ռ� ���� üũ
    MergeInfoResult CheckMergeOldWayInfo()
    {
        Debug.Log("���� ������� �ռ� ���� üũ ����");

        // ���� ���� üũ (����� �������)
        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        for (int i = 1; i < mergeCount; i++)
        {
            int currentCard = mergeCards[i].GetComponent<Card>().cardValue;
            Debug.Log($"ī�� ��: {firstCard} vs {currentCard}");

            if (currentCard != firstCard)
            {
                Debug.Log("���� ���ڰ� �ƴ� - �ռ� �Ұ�");
                return null; // ���� ���� �ƴ�
            }
        }

        // 16�� ī�� üũ
        if (firstCard == 16)
        {
            Debug.Log("16�� ī��� �� �̻� �ռ� �Ұ�");
            return null; // �� �̻� �ռ� �Ұ�
        }

        // Ȯ�� ��� (���� LuckyChance ���� ���)
        float chance = CalculateChanceForCard(firstCard, mergeCount);

        Debug.Log($"�ռ� ����! ī�尪: {firstCard}, ����: {mergeCount}, Ȯ��: {chance * 100}%");

        return new MergeInfoResult
        {
            ruleName = "���� ���� �ռ�",
            successRate = chance,
            reward = (firstCard + 1) * 1
        };
    }

    // GameManager.cs�� CalculateChanceForCard �޼��� �ϼ�
    float CalculateChanceForCard(int cardValue, int count)
    {
        float chance = 0f;

        switch (cardValue)
        {
            case 1: // Ŭ�ι� A
                if (count == 2) chance = 1.0f;
                else if (count == 3) chance = 0.97f;
                else if (count == 4) chance = 0.95f;
                break;

            case 2: // Ŭ�ι� J
                if (count == 2) chance = 0.92f;
                else if (count == 3) chance = 0.90f;
                else if (count == 4) chance = 0.90f;
                break;

            case 3: // Ŭ�ι� Q
                if (count == 2) chance = 0.89f;
                else if (count == 3) chance = 0.86f;
                else if (count == 4) chance = 0.85f;
                break;

            case 4: // Ŭ�ι� K
                if (count == 2) chance = 0.80f;
                else if (count == 3) chance = 0.78f;
                else if (count == 4) chance = 0.75f;
                break;

            case 5: // ���̾� A
                if (count == 2) chance = 0.73f;
                else if (count == 3) chance = 0.70f;
                else if (count == 4) chance = 0.69f;
                break;

            case 6: // ���̾� J
                if (count == 2) chance = 0.67f;
                else if (count == 3) chance = 0.65f;
                else if (count == 4) chance = 0.62f;
                break;

            case 7: // ���̾� Q
                if (count == 2) chance = 0.6f;
                else if (count == 3) chance = 0.58f;
                else if (count == 4) chance = 0.56f;
                break;

            case 8: // ���̾� K
                if (count == 2) chance = 0.5f;
                else if (count == 3) chance = 0.48f;
                else if (count == 4) chance = 0.46f;
                break;

            case 9: // ��Ʈ A
                if (count == 2) chance = 1f;
                else if (count == 3) chance = 0.8f;
                else if (count == 4) chance = 0.74f;
                break;

            case 10: // ��Ʈ J
                if (count == 2) chance = 0.44f;
                else if (count == 3) chance = 0.42f;
                else if (count == 4) chance = 0.4f;
                break;

            case 11: // ��Ʈ Q
                if (count == 2) chance = 0.38f;
                else if (count == 3) chance = 0.36f;
                else if (count == 4) chance = 0.34f;
                break;

            case 12: // ��Ʈ K
                if (count == 2) chance = 0.32f;
                else if (count == 3) chance = 0.3f;
                else if (count == 4) chance = 0.29f;
                break;

            case 13: // �����̵� A
                if (count == 2) chance = 0.28f;
                else if (count == 3) chance = 0.26f;
                else if (count == 4) chance = 0.24f;
                break;

            case 14: // �����̵� J
                if (count == 2) chance = 0.2f;
                else if (count == 3) chance = 0.15f;
                else if (count == 4) chance = 0.12f;
                break;

            case 15: // �����̵� Q
                if (count == 2) chance = 0.12f;
                else if (count == 3) chance = 0.11f;
                else if (count == 4) chance = 0.10f;
                break;

            case 16: // �����̵� K
                if (count == 2) chance = 0f;
                else if (count == 3) chance = 0f;
                else if (count == 4) chance = 0f;
                break;

            default:
                Debug.LogWarning($"ī�� Ÿ�� {cardValue}�� ���ǵ��� �ʾҽ��ϴ�.");
                chance = 0f;
                break;
        }

        Debug.Log($"ī�� {cardValue}, {count}�� �ռ� Ȯ��: {chance * 100}%");
        return chance;
    }


    void DeleteMergeCards()
    {
        Debug.Log($"DeleteMergeCards ȣ���! mergeCount: {mergeCount}");

        if (mergeCount == 0)
        {
            Debug.Log("������ ī�尡 �����ϴ�.");
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
        
        Debug.Log("Merge ������ ī����� �����Ǿ����ϴ�.");

        UpdateMergeInfo();
    }

    void MergeCards()
    {
        if (mergeCount != 2 && mergeCount != 3 && mergeCount != 4)
        {
            Debug.Log("�ռ��� �Ϸ��� ī�尡 2�� �Ǵ� 3�� Ȥ�� 4�� �ʿ��մϴ�.");
            SoundManager.Instance.PlayFullWarning();
            return;
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        // ��� ���ʽ� ���
        float totalBonus = 1.0f;
        CardEdition highestEdition = CardEdition.REGULAR;

        for (int i = 0; i < mergeCount; i++)
        {
            Card card = mergeCards[i].GetComponent<Card>();
            CardEdition cardEdition = card.GetCardEdition();

            // ���ʽ� ����
            if (CardRaritySystem.Instance != null)
            {
                totalBonus += CardRaritySystem.Instance.GetBonusMultiplier(cardEdition) - 1.0f;
            }

            // ���� ���� ��� ã��
            if (cardEdition > highestEdition)
            {
                highestEdition = cardEdition;
            }
        }

        int newValue = firstCard + 1;
        int baseScore = newValue * 1;
        int finalScore = Mathf.RoundToInt(baseScore * totalBonus);

        score += finalScore;
        Debug.Log($"�ռ� ����! ���� +{finalScore} (�⺻: {baseScore}, ���ʽ�: {totalBonus:F1}��)");

        // ī�� ����
        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                mergeCards[i].SetActive(false);
            }
        }

        // �� ī�� ����
        GameObject newCard = Instantiate(cardPrefab, mergeArea.position, Quaternion.identity);
        Card newCardComponent = newCard.GetComponent<Card>();

        if (newCardComponent != null)
        {
            int imageIndex = newValue - 1;
            newCardComponent.InitCard(newValue, cardImages[imageIndex]);

            // ��� ���׷��̵� �õ�
            if (CardRaritySystem.Instance != null)
            {
                float upgradeChance = CardRaritySystem.Instance.GetUpgradeChance(highestEdition);
                if (Random.Range(0f, 1f) < upgradeChance)
                {
                    CardEdition upgradedEdition = CardRaritySystem.Instance.UpgradeEdition(highestEdition);
                    newCardComponent.SetCardEdition(upgradedEdition);
                    Debug.Log($"��� ���׷��̵�! {upgradedEdition}");
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
                Debug.Log("�������� Ŭ����!");
                SoundManager.Instance.PlayRoundSuccess();
                StageManager.Instance.isGameCleared = true;

                // Ŭ���� ���� ����
                StartCoroutine(ShowClearEffect());
            }
        }
    }
   
    IEnumerator WaitAndLoadDialogue()
    {
        yield return new WaitForSeconds(2f); // 2�� ���
        StageManager.Instance.LoadDialogueScene();
    }

    void LuckyChance()
    {
        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        switch (firstCard)
        {
            case 1:              // Ŭ�ι�  A
                if (mergeCount == 2) chance = 1.0f;
                else if (mergeCount == 3) chance = 0.97f;
                else if (mergeCount == 4) chance = 0.95f;
                break;
                                   //Ŭ�ι� J
            case 2:
                if (mergeCount == 2) chance = 0.92f;
                else if (mergeCount == 3) chance = 0.90f;
                else if (mergeCount == 4) chance = 0.90f;
                break;

            case 3:                   //Ŭ�ι� Q
                if (mergeCount == 2) chance = 0.89f; // ����: 85f -> 0.85f
                else if (mergeCount == 3) chance = 0.86f;
                else if (mergeCount == 4) chance = 0.85f;
                break;

            case 4:                   //Ŭ�ι� K
                if (mergeCount == 2) chance = 0.80f;
                else if (mergeCount == 3) chance = 0.78f;
                else if (mergeCount == 4) chance = 0.75f;
                break;
                 
            case 5:                   //���̾� A
                if (mergeCount == 2) chance = 0.73f;
                else if (mergeCount == 3) chance = 0.70f;
                else if (mergeCount == 4) chance = 0.69f;
                break;

            case 6:                   //���̾� J
                if (mergeCount == 2) chance = 0.67f;
                else if (mergeCount == 3) chance = 0.65f;
                else if (mergeCount == 4) chance = 0.62f;
                break;

            case 7:                  //���̾� Q
                if (mergeCount == 2) chance = 0.6f;
                else if (mergeCount == 3) chance = 0.58f;
                else if (mergeCount == 4) chance = 0.56f;
                break;

            case 8:                   //���̾� k
                if (mergeCount == 2) chance = 0.5f;
                else if (mergeCount == 3) chance = 0.48f;
                else if (mergeCount == 4) chance = 0.46f;
                break;

            case 9:                 //��Ʈ A
                if (mergeCount == 2) chance = 1f;
                else if (mergeCount == 3) chance = 0.8f;
                else if (mergeCount == 4) chance = 0.74f;
                break;
                 
            case 10:                  //��Ʈ J
                if (mergeCount == 2) chance = 0.44f;
                else if (mergeCount == 3) chance = 0.42f;
                else if (mergeCount == 4) chance = 0.4f;
                break;

            case 11:                     //��Ʈ Q
                if (mergeCount == 2) chance = 0.38f;
                else if (mergeCount == 3) chance = 0.36f;
                else if (mergeCount == 4) chance = 0.34f;
                break;

            case 12:                //��Ʈ K
                if (mergeCount == 2) chance = 0.32f;
                else if (mergeCount == 3) chance = 0.3f;
                else if (mergeCount == 4) chance = 0.29f;
                break;

            case 13:             //�����̵� A
                if (mergeCount == 2) chance = 0.28f;
                else if (mergeCount == 3) chance = 0.26f;
                else if (mergeCount == 4) chance = 0.24f;
                break;

            case 14:             //�����̵� J
                if (mergeCount == 2) chance = 0.2f;
                else if (mergeCount == 3) chance = 0.15f;
                else if (mergeCount == 4) chance = 0.12f;
                break;

            case 15:             //�����̵� Q
                if (mergeCount == 2) chance = 0.12f;
                else if (mergeCount == 3) chance = 0.11f;
                else if (mergeCount == 4) chance = 0.10f;
                break;

            case 16:             //�����̵� K
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
        // ���� �߿��� ī�� �̵� �Ұ�
        if (!CanInteract())
        {
            Debug.Log("���� ���̹Ƿ� ī�� �̵��� ���ѵ˴ϴ�.");
            return;
        }

        if (mergeCount >= maxMergeSize)
        {
            Debug.Log("���� ������ ���� �����ϴ�!");
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
            StopAllCardDragging(); // ����Ʈ �����ϸ� ��� �巡�� ����
        }
    }

    // ��� ī�� �巡�� ����
    void StopAllCardDragging()
    {
        // ���� ī�� �巡�� ����
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

        // ���� ī�� �巡�� ����
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

    // ������ �� ����ϴ� ���� ī�� ��ġ
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
                    // �ε巯�� �ִϸ��̼� ������Ʈ Ȯ�� �� �߰�
                    SmoothCardAnimation smoothAnim = handCards[i].GetComponent<SmoothCardAnimation>();
                    if (smoothAnim == null)
                    {
                        smoothAnim = handCards[i].AddComponent<SmoothCardAnimation>();
                    }

                    // ���ҿ� ���� �̵� ���!
                    smoothAnim.SwapToPosition(targetPos);
                }
            }
        }
    }

    // ī�� ��ġ�� �������� �ٲٴ� �Լ� (���� �ִϸ��̼� ���)
    public void SwapHandCards(int index1, int index2)
    {
        if (index1 < 0 || index1 >= handCount || index2 < 0 || index2 >= handCount)
            return;

        if (handCards[index1] == null || handCards[index2] == null)
            return;

        // ī�� ��ġ ��ü
        GameObject temp = handCards[index1];
        handCards[index1] = handCards[index2];
        handCards[index2] = temp;

        // ���� ī�� ���ġ ���!
        ArrangeHandForSwap();

        Debug.Log($"ī�� {index1}���� {index2}�� ��ġ ��ü!");
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

// ��� ������ Ŭ����
public class MergeInfoResult
{
    public string ruleName;
    public float successRate;
    public int reward;
}