using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static MergeChanceSO;
using Unity.VisualScripting;

public class Card : MonoBehaviour
{
    public int cardValue;
    public Sprite cardImage;
    public TextMeshPro cardText;
    public MergeChanceSO mergeData;

    public void MergeData() //카드 데이터를 가져옴
    {
       
    }
    public void InitCard(int value, Sprite image)
    {
        cardValue = value;
        cardImage = image;

        GetComponent<SpriteRenderer>().sprite = image;

        if (cardText != null )
        {
            cardText.text = cardValue.ToString();
        }
    }

     

}
