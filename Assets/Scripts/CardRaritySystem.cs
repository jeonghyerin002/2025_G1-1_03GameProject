using UnityEngine;

public enum CardEdition
{
    REGULAR,     // 일반
    POLYCHROME,  // 중급
    NEGATIVE     // 고급
}

public class CardRaritySystem : MonoBehaviour
{
    public static CardRaritySystem Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // 등급별 보너스 점수 배수
    public float GetBonusMultiplier(CardEdition edition)
    {
        switch (edition)
        {
            case CardEdition.REGULAR: return 1.0f;
            case CardEdition.POLYCHROME: return 1.5f;
            case CardEdition.NEGATIVE: return 2.0f;
            default: return 1.0f;
        }
    }

    // 등급별 업그레이드 확률
    public float GetUpgradeChance(CardEdition edition)
    {
        switch (edition)
        {
            case CardEdition.REGULAR: return 0.7f;
            case CardEdition.POLYCHROME: return 0.5f;
            case CardEdition.NEGATIVE: return 0.3f;
            default: return 0.7f;
        }
    }

    // 등급 업그레이드
    public CardEdition UpgradeEdition(CardEdition currentEdition)
    {
        switch (currentEdition)
        {
            case CardEdition.REGULAR: return CardEdition.POLYCHROME;
            case CardEdition.POLYCHROME: return CardEdition.NEGATIVE;
            case CardEdition.NEGATIVE: return CardEdition.NEGATIVE; // 최고 등급
            default: return CardEdition.REGULAR;
        }
    }
}