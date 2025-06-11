using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageDataSO : ScriptableObject
{
    [Header("스테이지 정보")]
    public int stageNumber = 1;
    public string stageName = "스테이지 1";
    public int targetScore = 100;

    [Header("카드 덱 구성")]
    public int[] customDeck = new int[]
    {
        1,1,1,1,     // A 카드
        2,2,2,2,
        3,3,3,3,
        4,4,4,4,
        5,5,5,5
    };

    // 대사 데이터는 별도 씬에서 관리
}