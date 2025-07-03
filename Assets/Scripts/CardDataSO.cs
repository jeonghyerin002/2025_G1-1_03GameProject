using UnityEngine;

[CreateAssetMenu(fileName = "CardInfo", menuName = "Card/Card Info")]
public class CardInfoSO : ScriptableObject
{
    [Header("카드 기본 정보")]
    public int id;                  // 카드 고유 ID (1~52)
    public string cardName;         // "스페이드 A", "하트 K" 등
    public Sprite cardSprite;       // 카드 이미지

    [Header("트럼프 정보")]
    public CardSuit suit;           // 무늬 (스페이드, 하트, 다이아, 클럽)
    public int number;              // 숫자 (1=A, 11=J, 12=Q, 13=K)

    [Header("게임 정보")]
    public int gameValue;           // 게임에서 사용할 값
    public int baseScore;           // 기본 점수
}

public enum CardSuit
{
    Spade,      // 스페이드
    Heart,      // 하트
    Diamond,    // 다이아
    Club        // 클럽
}