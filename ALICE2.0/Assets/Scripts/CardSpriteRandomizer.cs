using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CardSpriteRandomizer : MonoBehaviour
{
    private static List<Sprite> remainingSprites;

    void Start()
    {
        // Load and shuffle only once
        if (remainingSprites == null || remainingSprites.Count == 0)
        {
            remainingSprites = Resources.LoadAll<Sprite>("Cards").OrderBy(x => Random.value).ToList();
        }

        if (remainingSprites.Count == 0)
        {
            Debug.LogWarning("No more unique sprites left!");
            return;
        }

        Sprite sprite = remainingSprites[0];
        remainingSprites.RemoveAt(0);

        GetComponent<Image>().sprite = sprite;
        gameObject.name = sprite.name;
    }
}




