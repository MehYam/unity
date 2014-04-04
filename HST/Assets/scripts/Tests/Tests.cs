using UnityEngine;
using System.Collections.Generic;

public sealed class Tests : MonoBehaviour
{
    void Start()
    {
        TestDeck();

        TestGameSimple();
    }

    static void LogResult(string name, bool pass)
    {
        if (pass)
        {
            Debug.Log(name + ": passed");
        }
        else
        {
            Debug.LogError(name + ": FAILED");
        }
    }

    void TestDeck()
    {
        Debug.Log("Tests.TestDeck");

        var deck = new Deck();
        deck.CreateRandom();

        // test shuffle
        deck.Shuffle();

        // test draw
    }

    void TestGameSimple()
    {
        var game = new Game();

        game.heros[0].deck.CreateRandom();
        game.heros[1].deck.CreateRandom();

      
    }
}
