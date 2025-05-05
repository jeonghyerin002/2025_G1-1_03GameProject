using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MargeData : MonoBehaviour
{
    public class SimpleMergeChanceWithSwitch
    {
        public bool TryMerge(string cardType, int count)
        {
            float chance = 0f;


            switch (cardType)
            {
                case "Card1":
                    if (count == 2) chance = 1.0f;
                    else if (count == 3) chance = 0.97f;
                    else if (count == 4) chance = 0.95f;
                    break;

                case "Card2":
                    if (count == 2) chance = 0.92f;
                    else if (count == 3) chance = 0.90f;
                    else if (count == 4) chance = 0.87f;
                    break;

                case "Card3":
                    if (count == 2) chance = 85f;
                    else if (count == 3) chance = 0.82f;
                    else if (count == 4) chance = 0.80f;
                    break;

                case "Card4":
                    if (count == 2) chance = 0.75f;
                    else if (count == 3) chance = 0.72f;
                    else if (count == 4) chance = 0.70f;
                    break;

                case "Card5":
                    if (count == 2) chance = 0.5f;
                    else if (count == 3) chance = 0.45f;
                    else if (count == 4) chance = 0.44f;
                    break;

                case "Card6":
                    if (count == 2) chance = 0.42f;
                    else if (count == 3) chance = 0.40f;
                    else if (count == 4) chance = 0.39f;
                    break;

                case "Card7":
                    if (count == 2) chance = 1.0f;
                    else if (count == 3) chance = 0.8f;
                    else if (count == 4) chance = 0.74f;
                    break;

                case "Card8":
                    if (count == 2) chance = 0.4f;
                    else if (count == 3) chance = 0.38f;
                    else if (count == 4) chance = 0.35f;
                    break;

                case "Card9":
                    if (count == 2) chance = 0.25f;
                    else if (count == 3) chance = 0.23f;
                    else if (count == 4) chance = 0.2f;
                    break;

                case "Card10":
                    if (count == 2) chance = 0.19f;
                    else if (count == 3) chance = 0.19f;
                    else if (count == 4) chance = 0.17f;
                    break;

                case "Card11":
                    if (count == 2) chance = 0.1f;
                    else if (count == 3) chance = 0.08f;
                    else if (count == 4) chance = 0.05f;
                    break;

                case "Card12":
                    if (count == 2) chance = 0.1f;
                    else if (count == 3) chance = 0.08f;
                    else if (count == 4) chance = 0.05f;
                    break;

                case "Card13":
                    if (count == 2) chance = 0.1f;
                    else if (count == 3) chance = 0.08f;
                    else if (count == 4) chance = 0.05f;
                    break;

                default:
                    Debug.LogWarning($"카드 타입 {cardType}는 정의되지 않았습니다.");
                    break;
            }

            return UnityEngine.Random.value < chance;

        }
    }
}
