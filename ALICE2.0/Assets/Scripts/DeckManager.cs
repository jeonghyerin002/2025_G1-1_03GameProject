using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private List<string> suits = new() { "S", "H", "D", "C" };
    private List<string> ranks = new() { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    private List<string> currentDeck;

    public static DeckManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        ResetDeck();
    }

    public void ResetDeck()
    {
        currentDeck = suits.SelectMany(s => ranks.Select(r => r + s)).ToList();
        Shuffle(currentDeck);
    }

    public List<string> DrawCards(int count)
    {
        if (currentDeck.Count < count)
        {
            Debug.LogWarning("Not enough cards left in the deck!");
            count = currentDeck.Count;
        }

        List<string> drawn = currentDeck.Take(count).ToList();
        currentDeck.RemoveRange(0, count);
        return drawn;
    }

    private void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}

