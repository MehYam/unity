using System.Collections.Generic;

public class Deck
{
    // premature optimization:  deck is stored as an array of int
    readonly long[] cards = new long[30];
    readonly long[] drawnCards = new long[30];

    // KAI: something like this...
    public void AddCard();
    public void RemoveCard();

    public void Shuffle();
    public void Mulligan();
    public Card3 Draw();
}
