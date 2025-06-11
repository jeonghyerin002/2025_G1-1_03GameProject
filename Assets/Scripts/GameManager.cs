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
        return !isPlayingEffect && !isMergeEffectPlaying && !isDeleteEffectPlaying && !isClearEffectPlaying;
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
    }

    void InitializeStage()
    {
        if (StageManager.Instance != null)
        {
            StageDataSO currentStageData = StageManager.Instance.GetCurrentStageData();

            // ���� ������������ ����
            prefedinedDeck = currentStageData.customDeck;

            // UI ������Ʈ
            if (stageText != null)
                stageText.text = "Stage " + StageManager.Instance.currentStage;

            if (targetScoreText != null)
                targetScoreText.text = "��ǥ: " + currentStageData.targetScore + "��";

            Debug.Log($"�������� {StageManager.Instance.currentStage} ����!");
        }

        // �迭 �ʱ�ȭ
        deckCards = new GameObject[prefedinedDeck.Length];
        handCards = new GameObject[maxHandSize];
        mergeCards = new GameObject[maxMergeSize];

        InitializeDeck();
        ShuffleDeck();
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
            StartCoroutine(ShowDeleteEffect());
        }
        else
        {
            Debug.Log("������ ī�尡 �����ϴ�. ���� ������ ī�带 ���� �־��ּ���!");
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
        Debug.Log($"���� ��ư Ŭ����! CanInteract: {CanInteract()}");

        // ��ȣ�ۿ� �Ұ����� ���¸� ����
        if (!CanInteract())
        {
            Debug.Log("���� ���̹Ƿ� ���� ��ư ���õ�");
            return;
        }

        // ī�� ���� ������ ���� Ȯ��
        if (mergeCount < 2)
        {
            Debug.Log("�����Ϸ��� �ּ� 2���� ī�尡 �ʿ��մϴ�.");
            return;
        }

        int value = mergeCards[0].GetComponent<Card>().cardValue;
        for (int i = 1; i < mergeCount; i++)
        {
            if (mergeCards[i].GetComponent<Card>().cardValue != value)
            {
                Debug.Log("���� ������ ī�常 ���� �� �� �ֽ��ϴ�.");
                ShowWarningMessage("���� ������ ī�常 ���� ����!", Color.yellow);
                return;
            }
        }

        int firstCard = mergeCards[0].GetComponent<Card>().cardValue;

        if (firstCard == 13)
        {
            ShowWarningMessage("�� �̻� �ռ� �� �� �����ϴ�.", Color.red);
            Debug.Log("13�� ī���̹Ƿ� �ռ� �Ұ�");
            return;
        }

        // ���� ����
        Debug.Log("���� ����! ��ư�� ��Ȱ��ȭ");
        isPlayingEffect = true;
        SetButtonsInteractable(false);

        float GoodChance = Random.value;
        LuckyChance();

        Debug.Log($"Ȯ��: {chance}, ������: {GoodChance}");

        if (GoodChance <= chance)
        {
            // ����!
            int newValue = firstCard + 1;
            int scoreToAdd = newValue * 1;

            // ���̴� ȿ�� ���� ����
            ApplyMergeSuccessEffects();

            StartCoroutine(ShowMergeSuccess(newValue, scoreToAdd));

            // ��� �� ���� ���� ó�� (����������)
            StartCoroutine(DelayedMergeCards());

            Debug.Log("���� �߽��ϴ�!!!!!!!!!!!!!!");
        }
        else
        {
            // ����!
            // ���̴� ȿ�� ���� ����
            ApplyMergeFailureEffects();

            Debug.Log("�����߽��ϴ�!!!!!!!!!!!!");
            StartCoroutine(ShowMergeFailure());

            // ��� �� ī�� ���� (����������)
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
        Debug.Log("DelayedDeleteMergeCards ����");

        // ���� ȿ�� �Ϸ���� ���
        yield return new WaitUntil(() => !isMergeEffectPlaying);

        yield return new WaitForSeconds(1.2f); // ���� �ð� ���

        Debug.Log("���� DeleteMergeCards ȣ��");
        DeleteMergeCards();

        // ���� ���� �� ��ư ����
        Debug.Log("���� �Ϸ� - ��ư ����");
        RestoreButtonsAfterEffect();
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
            return;
        }

        if (deckCount <= 0)
        {
            ShowWarningMessage("�� �̻� ī�带 ���� �� ���� �� ����", Color.red);
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
            // ���� ���� �ƴϰ�, ī�尡 2~3�� ���� ���� Ȱ��ȭ
            bool canMerge = CanInteract() && (mergeCount == 2 || mergeCount == 3);
            mergeButton.interactable = canMerge;
        }
    }

    void DeleteMergeCards()
    {
        Debug.Log($"DeleteMergeCards ȣ���! mergeCount: {mergeCount}");

        if (mergeCount == 0)
        {
            Debug.Log("������ ī�尡 �����ϴ�.");
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

        int newValue = firstCard + 1;
        int scoreToAdd = newValue * 1;
        score += scoreToAdd;
        Debug.Log($"���� ����! ���� +{scoreToAdd} (���� ����: {score})");

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

        // �� ī���� ũ�⸦ ���� �����հ� �����ϰ� ����
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
                if (mergeCount == 2) chance = 0.85f; // ����: 85f -> 0.85f
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
    }
}