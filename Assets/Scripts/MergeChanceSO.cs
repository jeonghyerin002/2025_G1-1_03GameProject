using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


    [CreateAssetMenu(fileName = "MergeChanceSO", menuName = "Merge System/MergeChanceData")]
    public class MergeChanceSO: ScriptableObject
    {
        [System.Serializable]
        public class CardMergeChance
        {
            public string cardName;

            public float[] mergeChances;

        }

        public CardMergeChance[]
            cardMergeChances;

    }