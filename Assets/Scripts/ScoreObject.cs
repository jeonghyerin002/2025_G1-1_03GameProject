using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreObject : MonoBehaviour             //게임엔진입문 11강 5p
{
    [SerializeField] ScoreSO data;

    public int GetScore()
    {
        return data.score;
    }
}
