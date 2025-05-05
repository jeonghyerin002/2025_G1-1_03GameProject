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
            Debug.Log("�ռ� ����");

            if (currentIndex += 1);
            GetComponent<SpriteRenderer>().sprite = cardSprites[currentIndex];
            Debug.Log($"ī�尡 Card{{currentIndex + 1}}�� ��ȭ�߽��ϴ�!")

             else
            {
                Debug.Log("���� �ܰ� ī���Դϴ�.");
            }
        else
            {
                Debug.Log("�ռ� ����");
            }
        }
        
    }
}
