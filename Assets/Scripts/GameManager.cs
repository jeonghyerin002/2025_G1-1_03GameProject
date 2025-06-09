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

    // ���� ���� ����
    private bool isPlayingEffect = false;

    // DragDrop���� ȣ���� �� �ֵ��� public �Լ� �߰�
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
            DeleteButton.onClick.AddListener(() => {
                Debug.Log($"���� ��ư Ŭ��! ���� mergeCount: {mergeCount}");

                if (mergeCount > 0)
                {
                    Debug.Log("���� ���� ����!");
                    StartCoroutine(ShowDeleteEffect());
                    // DelayedDeleteMergeCards() ����! ShowDeleteEffect���� ��ü������ ó��
                }
                else
                {
                    Debug.Log("������ ī�尡 �����ϴ�. ���� ������ ī�带 ���� �����ּ���!");

                    // ������ ī�尡 ���� ���� �ణ�� �ǵ�� ����
                    if (mergeResultText != null)
                    {
                        mergeResultText.text = "������ ī�尡 �����!";
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

        // ���� ��� �ؽ�Ʈ �ʱ�ȭ
        if (mergeResultText != null)
        {
            mergeResultText.gameObject.SetActive(false);
        }
    }

    // ���� ���� ���̴� ȿ�� ����
    void ApplyMergeSuccessEffects()
    {
        Debug.Log($"���� ȿ�� ���� ����! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            Debug.Log($"���� ī�� {i} Ȯ�� ��...");

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

                    // Card ������Ʈ�� ��� �⺻ ȿ���� �����ֱ�
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
                Debug.LogError($"mergeCards[{i}]�� null�Դϴ�!");
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

    // ���� ���� ���� (���̴� ȿ�� ����, �⺻ UI��)
    IEnumerator ShowMergeSuccess(int newCardValue, int scoreGained)
    {
        // ���� �ؽ�Ʈ ǥ��
        if (mergeResultText != null)
        {
            mergeResultText.text = $"����! +{scoreGained}��";
            mergeResultText.color = Color.green;
            mergeResultText.gameObject.SetActive(true);

            // �ؽ�Ʈ �ִϸ��̼�
            mergeResultText.transform.localScale = Vector3.zero;
            mergeResultText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.3f);

            mergeResultText.transform.DOScale(1f, 0.2f);

            yield return new WaitForSeconds(1f);

            // ���̵� �ƿ�
            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // ���� ����Ʈ Ȱ��ȭ
        if (mergeSuccessEffect != null)
        {
            mergeSuccessEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeSuccessEffect.SetActive(false);
        }
    }

    // ���� ���� ���� (���̴� ȿ�� ����, �⺻ UI��)
    IEnumerator ShowMergeFailure()
    {
        // ���� �ؽ�Ʈ ǥ��
        if (mergeResultText != null)
        {
            mergeResultText.text = "����!";
            mergeResultText.color = Color.red;
            mergeResultText.gameObject.SetActive(true);

            // ���� �ִϸ��̼�
            mergeResultText.transform.DOShakePosition(1f, strength: 30f, vibrato: 20);

            yield return new WaitForSeconds(1.5f);

            // ���̵� �ƿ�
            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // ���� ����Ʈ Ȱ��ȭ
        if (mergeFailEffect != null)
        {
            mergeFailEffect.SetActive(true);
            yield return new WaitForSeconds(2f);
            mergeFailEffect.SetActive(false);
        }
    }

    // ���� ����
    IEnumerator ShowDeleteEffect()
    {
        // ���� �ؽ�Ʈ ǥ��
        if (mergeResultText != null)
        {
            mergeResultText.text = "ī�� ����!";
            mergeResultText.color = new Color(1f, 0.5f, 0f); // ��Ȳ��
            mergeResultText.gameObject.SetActive(true);

            // �޽� �ִϸ��̼�
            mergeResultText.transform.localScale = Vector3.one;
            mergeResultText.transform.DOScale(1.3f, 0.3f).SetLoops(3, LoopType.Yoyo);

            yield return new WaitForSeconds(1f);

            // ���̵� �ƿ�
            mergeResultText.DOFade(0f, 0.5f).OnComplete(() => {
                mergeResultText.gameObject.SetActive(false);
                mergeResultText.color = new Color(mergeResultText.color.r, mergeResultText.color.g, mergeResultText.color.b, 1f);
            });
        }

        // ī��鿡 ���� ���̴� ȿ�� ���� (mergeCount�� 0�� �Ǳ� ����!)
        Debug.Log($"���� ȿ�� ���� ����! mergeCount: {mergeCount}");

        for (int i = 0; i < mergeCount; i++)
        {
            if (mergeCards[i] != null)
            {
                Debug.Log($"ī�� {i} Ȯ�� ��...");

                Card cardComponent = mergeCards[i].GetComponent<Card>();
                if (cardComponent != null)
                {
                    Debug.Log($"ī�� {i}�� Card ������Ʈ �߰�! ���� ȿ�� ���� ��...");

                    // �� ī�帶�� �ణ�� ������
                    float delay = i * 0.1f;
                    StartCoroutine(DelayedDeleteEffect(cardComponent, delay));
                }
                else
                {
                    Debug.LogError($"ī�� {i}�� Card ������Ʈ�� �����ϴ�!");
                }
            }
            else
            {
                Debug.LogError($"mergeCards[{i}]�� null�Դϴ�!");
            }
        }

        // ��� �ִϸ��̼��� ���� ������ ���
        yield return new WaitForSeconds(0.5f + (mergeCount * 0.1f) + 1f); // �����ð� �߰�

        // ���� ������ ī����� ����
        Debug.Log("ShowDeleteEffect���� ī�� ���� (DelayedDeleteMergeCards������ ó����)");
        // DeleteMergeCards(); ���� - DelayedDeleteMergeCards���� ó��

        // ��ư ������ DelayedDeleteMergeCards���� ó��
        // RestoreButtonsAfterEffect(); ����
    }

    // ������ ���� ȿ��
    IEnumerator DelayedDeleteEffect(Card card, float delay)
    {
        Debug.Log($"DelayedDeleteEffect ����! delay: {delay}");
        yield return new WaitForSeconds(delay);

        if (card != null)
        {
            Debug.Log($"Card.PlayDeleteEffect() ȣ��!");
            card.PlayDeleteEffect();
        }
        else
        {
            Debug.LogError("Card�� null�Դϴ�!");
        }
    }

    public void OnMergeButtonClicked()
    {
        Debug.Log($"���� ��ư Ŭ����! isPlayingEffect: {isPlayingEffect}");

        // ���� ���̸� ����
        if (isPlayingEffect)
        {
            Debug.Log("���� ���̹Ƿ� ���� ��ư ���õ�");
            return;
        }

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

        // ���� ����
        Debug.Log("���� ����! ��ư�� ��Ȱ��ȭ");
        isPlayingEffect = true;
        SetButtonsInteractable(false);

        float GoodChance = Random.value;
        LuckyChance();

        Debug.Log(chance);
        Debug.Log("������: " + GoodChance);

        if (GoodChance <= chance)
        {
            // ����!
            int newValue = firstCard + 1;
            int scoreToAdd = newValue * 1;

            // ���̴� ȿ�� ���� ����
            ApplyMergeSuccessEffects();

            StartCoroutine(ShowMergeSuccess(newValue, scoreToAdd));

            // ��� �� ���� ���� ó�� (�������)
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

            // ��� �� ī�� ���� (�������)
            StartCoroutine(DelayedDeleteMergeCards());
        }
    }

    // ������ ���� ó�� (���� ��) - ������� ����
    IEnumerator DelayedMergeCards()
    {
        Debug.Log("DelayedMergeCards ����");
        yield return new WaitForSeconds(0.5f); // ���� �ð� ���

        Debug.Log("���� MergeCards ȣ��");
        MergeCards();

        // ���� ���� �� ��ư ����
        Debug.Log("���� �Ϸ� - ��ư ����");
        RestoreButtonsAfterEffect();
    }

    // ������ ī�� ���� (���� ��) - ������� ����  
    IEnumerator DelayedDeleteMergeCards()
    {
        Debug.Log("DelayedDeleteMergeCards ����");
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

        if (mergeButton != null)
            mergeButton.interactable = interactable;

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
        Debug.Log($"�̱� ��ư Ŭ����! isPlayingEffect: {isPlayingEffect}");

        // ���� ���̸� ����
        if (isPlayingEffect)
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
        Debug.Log($"DeleteMergeCards ȣ���! mergeCount: {mergeCount}");

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
        }

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
        if (score >= roundSOs[gameRound - 1].score)
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

    void dontmerge()
    {
        maxcard.gameObject.SetActive(false);
    }
}