using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public Sprite[] cardSprites;

    private int currentIndex = 2;

    public bool TryMergeSuccess()
    {
        return Random.value < currentIndex;
    }

    public void TryMerge()
    {
        if (TryMergeSuccess(0))
        {
            Debug.Log("합성 성공");

            if (currentIndex += 1);
            GetComponent<SpriteRenderer>().sprite = cardSprites[currentIndex];
            Debug.Log($"카드가 Card{{currentIndex + 1}}로 진화했습니다!")

             else
            {
                Debug.Log("최종 단계 카드입니다.");
            }
        else
            {
                Debug.Log("합성 실패");
            }
        }
        
    }
}
