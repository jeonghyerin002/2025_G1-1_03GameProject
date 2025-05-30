using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Round", fileName = "RoundScore")]
public class RoundSO : ScriptableObject
{
    public int score;
    public int reward;
}
