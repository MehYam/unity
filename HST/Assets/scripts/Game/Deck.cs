using System;
using System.Collections.Generic;
using System.Text;

public class Deck
{
    static public readonly int SIZE = 30;
    readonly AbstractAbility[] _cards = new AbstractAbility[SIZE];
    int _next = 0;

    public AbstractAbility[] cards { get { return _cards; } }
    public void CreateRandom()
    {
        _next = 0;        
        for (int i = 0; i < _cards.Length; ++i)
        {
            _cards[i] = AbilityFactory.Instance.CreateRandomMinionCard();
        }
    }

    public void Shuffle() 
    {
        _next = 0;
        var rnd = new Random();
        for (int i = 0; i < SIZE; ++i)
        {
            var randomIndex = rnd.Next(0, _cards.Length);
            var randomPick = _cards[randomIndex];
            _cards[randomIndex] = _cards[i];
            _cards[i] = randomPick;
        }
    }
    public int remaining { get { return SIZE - _next; } }
    public AbstractAbility Draw()
    {
        return remaining > 0 ? _cards[_next++] : null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var card in _cards)
        {
            if (card == null) break;
            sb.AppendLine(card.ToString());
        }
        return sb.ToString();
    }
}
