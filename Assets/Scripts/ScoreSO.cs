using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Score", fileName = "BestScore")]

public class ScoreSO : ScriptableObject
{
    [Header("Score Value")]

    public int score = 0;
}
