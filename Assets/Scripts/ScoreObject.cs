using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreObject : MonoBehaviour             //���ӿ����Թ� 11�� 5p
{
    [SerializeField] ScoreSO data;

    public int Getpoint()
    {
        return data.score;
    }
}
