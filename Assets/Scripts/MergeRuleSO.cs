using UnityEngine;

[CreateAssetMenu(fileName = "MergeRule", menuName = "Card/Merge Rule")]
public class MergeRuleSO : ScriptableObject
{
    [Header("규칙 정보")]
    public string ruleName;         // "같은 숫자", "스트레이트" 등
    public bool isActive = true;    // 활성화 여부

    [Header("조건")]
    public RuleType ruleType;       // 규칙 타입
    public int minCards = 2;        // 최소 카드 수
    public int maxCards = 4;        // 최대 카드 수

    [Header("성공 확률 (카드 수별)")]
    public float[] successRates = { 0.8f, 0.7f, 0.6f }; // 2장, 3장, 4장

    [Header("보상")]
    public int scoreReward = 10;    // 점수 보상
    public int newCardValue = 0;    // 새로 생성할 카드 값 (0이면 +1)
}

public enum RuleType
{
    SameNumber,     // 같은 숫자
    SameSuit,       // 같은 무늬  
    Straight,       // 연속 숫자
    StraightFlush,  // 같은 무늬 + 연속 숫자
    Pair,           // 페어 (정확히 2장)
    Triple,         // 트리플 (정확히 3장)
    Custom          // 사용자 정의
}