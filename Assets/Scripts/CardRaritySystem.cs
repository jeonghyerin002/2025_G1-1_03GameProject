using UnityEngine;

public enum CardEdition
{
    REGULAR,     // �Ϲ�
    POLYCHROME,  // �߱�
    NEGATIVE     // ���
}

public class CardRaritySystem : MonoBehaviour
{
    public static CardRaritySystem Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // ��޺� ���ʽ� ���� ���
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

    // ��޺� ���׷��̵� Ȯ��
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

    // ��� ���׷��̵�
    public CardEdition UpgradeEdition(CardEdition currentEdition)
    {
        switch (currentEdition)
        {
            case CardEdition.REGULAR: return CardEdition.POLYCHROME;
            case CardEdition.POLYCHROME: return CardEdition.NEGATIVE;
            case CardEdition.NEGATIVE: return CardEdition.NEGATIVE; // �ְ� ���
            default: return CardEdition.REGULAR;
        }
    }
}