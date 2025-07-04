using UnityEngine;

[CreateAssetMenu(fileName = "DeckComposition", menuName = "Card/Deck Composition")]
public class DeckCompositionSO : ScriptableObject
{
    [Header("�� ����")]
    public string deckName = "�⺻ ��";
    public string description = "�� ����";

    [Header("ī�� ����")]
    public DeckCard[] deckCards;

    [Header("�� ����")]
    public bool shuffleOnStart = true;      // ���۽� ���� ����
    public int maxDeckSize = 52;            // �ִ� �� ũ��

    // ��ü ī�� �� ���
    public int GetTotalCardCount()
    {
        int total = 0;
        foreach (var deckCard in deckCards)
        {
            total += deckCard.count;
        }
        return total;
    }

    // ���� �迭�� ��ȯ
    public CardInfoSO[] GetDeckAsArray()
    {
        int totalCount = GetTotalCardCount();
        CardInfoSO[] result = new CardInfoSO[totalCount];

        int index = 0;
        foreach (var deckCard in deckCards)
        {
            for (int i = 0; i < deckCard.count; i++)
            {
                if (index < result.Length)
                {
                    result[index] = deckCard.cardInfo;
                    index++;
                }
            }
        }

        return result;
    }

    // ���� int �迭 ������ε� ��ȯ (ȣȯ��)
    public int[] GetDeckAsIntArray()
    {
        int totalCount = GetTotalCardCount();
        int[] result = new int[totalCount];

        int index = 0;
        foreach (var deckCard in deckCards)
        {
            for (int i = 0; i < deckCard.count; i++)
            {
                if (index < result.Length && deckCard.cardInfo != null)
                {
                    result[index] = deckCard.cardInfo.gameValue;
                    index++;
                }
            }
        }

        return result;
    }
}

[System.Serializable]
public class DeckCard
{
    public CardInfoSO cardInfo;     // ī�� ����
    public int count = 1;           // ����

    [Header("ǥ�ÿ� (�б� ����)")]
    [SerializeField] private string displayName;

    void OnValidate()
    {
        // Inspector���� ī�� �̸� ǥ��
        if (cardInfo != null)
        {
            displayName = $"{cardInfo.cardName} x{count}";
        }
        else
        {
            displayName = "ī�� ����";
        }
    }
}