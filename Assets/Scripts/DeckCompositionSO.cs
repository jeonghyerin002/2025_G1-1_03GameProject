using UnityEngine;

[CreateAssetMenu(fileName = "DeckComposition", menuName = "Card/Deck Composition")]
public class DeckCompositionSO : ScriptableObject
{
    [Header("덱 정보")]
    public string deckName = "기본 덱";
    public string description = "덱 설명";

    [Header("카드 구성")]
    public DeckCard[] deckCards;

    [Header("덱 설정")]
    public bool shuffleOnStart = true;      // 시작시 셔플 여부
    public int maxDeckSize = 52;            // 최대 덱 크기

    // 전체 카드 수 계산
    public int GetTotalCardCount()
    {
        int total = 0;
        foreach (var deckCard in deckCards)
        {
            total += deckCard.count;
        }
        return total;
    }

    // 덱을 배열로 변환
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

    // 기존 int 배열 방식으로도 변환 (호환성)
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
    public CardInfoSO cardInfo;     // 카드 정보
    public int count = 1;           // 개수

    [Header("표시용 (읽기 전용)")]
    [SerializeField] private string displayName;

    void OnValidate()
    {
        // Inspector에서 카드 이름 표시
        if (cardInfo != null)
        {
            displayName = $"{cardInfo.cardName} x{count}";
        }
        else
        {
            displayName = "카드 없음";
        }
    }
}