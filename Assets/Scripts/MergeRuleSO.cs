using UnityEngine;

[CreateAssetMenu(fileName = "MergeRule", menuName = "Card/Merge Rule")]
public class MergeRuleSO : ScriptableObject
{
    [Header("��Ģ ����")]
    public string ruleName;         // "���� ����", "��Ʈ����Ʈ" ��
    public bool isActive = true;    // Ȱ��ȭ ����

    [Header("����")]
    public RuleType ruleType;       // ��Ģ Ÿ��
    public int minCards = 2;        // �ּ� ī�� ��
    public int maxCards = 4;        // �ִ� ī�� ��

    [Header("���� Ȯ�� (ī�� ����)")]
    public float[] successRates = { 0.8f, 0.7f, 0.6f }; // 2��, 3��, 4��

    [Header("����")]
    public int scoreReward = 10;    // ���� ����
    public int newCardValue = 0;    // ���� ������ ī�� �� (0�̸� +1)
}

public enum RuleType
{
    SameNumber,     // ���� ����
    SameSuit,       // ���� ����  
    Straight,       // ���� ����
    StraightFlush,  // ���� ���� + ���� ����
    Pair,           // ��� (��Ȯ�� 2��)
    Triple,         // Ʈ���� (��Ȯ�� 3��)
    Custom          // ����� ����
}