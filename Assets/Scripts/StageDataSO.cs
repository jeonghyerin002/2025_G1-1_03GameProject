using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageDataSO : ScriptableObject
{
    [Header("�������� ����")]
    public int stageNumber = 1;
    public string stageName = "�������� 1";
    public int targetScore = 100;

    [Header("ī�� �� ����")]
    public int[] customDeck = new int[]
    {
        1,1,1,1,     // A ī��
        2,2,2,2,
        3,3,3,3,
        4,4,4,4,
        5,5,5,5
    };

    // ��� �����ʹ� ���� ������ ����
}