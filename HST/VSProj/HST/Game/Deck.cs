using System;
using System.Collections.Generic;
using System.Text;

using HST.Util;

public class Deck<T>
{
    readonly T[] _cards;
    int _next = 0;
    public Deck(int size)
    {
        _cards = new T[size];
    }

    public void Shuffle() 
    {
        // this shuffles undrawn cards.
        var rnd = new Random();
        var size = _cards.Length;
        for (int i = _next; i < size; ++i)
        {   
            var randomIndex = rnd.Next(0, _cards.Length);
            var randomPick = _cards[randomIndex];
            _cards[randomIndex] = _cards[i];
            _cards[i] = randomPick;
        }
    }
    public int remaining { get { return _cards.Length - _next; } }
    public T Draw()
    {
        return remaining > 0 ? _cards[_next++] : default(T);
    }

    public void SetCardAt(int index, T card)
    {
        DebugUtils.Assert(index >= 0 && index < _cards.Length);

        _cards[index] = card;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        var size = _cards.Length;
        for (int i = _next; i < size; ++i)
        {
            sb.AppendLine(_cards[i].ToString());
        }
        return sb.ToString();
    }
}

public class Hand<T> : IEnumerable<T>
{
    LinkedList<T> cards = new LinkedList<T>();

    public int size { get { return cards.Count; } }
    public void AddCard(T card)
    {
        cards.AddLast(card);
    }
    public T PullFirst()
    {
        DebugUtils.Assert(size > 0);

        var first = cards.First.Value;
        cards.RemoveFirst();

        return first;
    }
    public void PullCard(T card)
    {
        //KAI: there's no remove-nth?  That's probably what I'll need...
        var removed = cards.Remove(card);

        DebugUtils.Assert(removed);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return cards.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return cards.GetEnumerator();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var card in this)
        {
            sb.AppendLine(card.ToString());
        }
        return sb.ToString();
    }
}
