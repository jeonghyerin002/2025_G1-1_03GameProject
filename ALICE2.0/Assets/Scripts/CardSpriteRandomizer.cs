using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CardSpriteRandomizer : MonoBehaviour
{
    void Start()
    {
        // Load all card sprites from Resources/Cards
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Cards");
        if (allSprites.Length == 0)
        {
            Debug.LogError("No sprites found in Resources/Cards!");
            return;
        }

        // Pick one randomly
        Sprite randomSprite = allSprites[Random.Range(0, allSprites.Length)];

        // Apply it to this Image component
        GetComponent<Image>().sprite = randomSprite;
        gameObject.name = randomSprite.name;
    }
}

