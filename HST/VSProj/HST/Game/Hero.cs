using System.Collections.Generic;
using HST.Util;

public sealed class Hero
{
    public int health { get; private set; }
    public int mana { get; private set; }

    public Deck<AbstractCard> deck { get; private set; }
    public Hand<AbstractCard> hand { get; private set; }
    public Playfield field { get; private set; }

    Hero()
    {
        health = 30;
        mana = 1;

        deck = new Deck<AbstractCard>(Game.DECKSIZE);
        hand = new Hand<AbstractCard>();
        field = new Playfield();
    }

    public enum CLASS { MAGE, WARRIOR, PRIEST, PALADIN, WARLOCK, DRUID, HUNTER, SHAMAN, ROGUE };
    static public Hero CreateHero(CLASS heroClass)
    {
        return new Hero();
    }

    public void Draw(int cards)
    {
        for (int i = 0; i < cards; ++i)
        {
            var card = deck.Draw();
            hand.AddCard(card);
        }
    }

    public void Mulligan(Hand<AbstractCard> cardsToMully)
    {
        DebugUtils.Assert(cardsToMully.size < Game.DECKSIZE);

        // rebuild a new deck with the undrawn and mully'ed cards, then shuffle it
        var newDeck = new Deck<AbstractCard>(Game.DECKSIZE - hand.size + cardsToMully.size);
        int newDeckIndex = 0;
        while (deck.remaining > 0)
        {
            newDeck.SetCardAt(newDeckIndex++, deck.Draw());
        }
        foreach (var card in cardsToMully)
        {
            newDeck.SetCardAt(newDeckIndex++, card);
            hand.PullCard(card);
        }
        newDeck.Shuffle();
        deck = newDeck;

        DebugUtils.Assert(newDeckIndex == Game.DECKSIZE && newDeck.remaining + hand.size == Game.DECKSIZE);
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Deck cards remaining: " + deck.remaining);
        sb.AppendLine(deck.ToString());
        sb.AppendLine("Hand:");
        sb.AppendLine(hand.ToString());
        sb.AppendLine("Playfield:");
        sb.AppendLine(field.ToString());

        return sb.ToString();
    }
}

public class Playfield
{
    public readonly MinionCard[] minions = new MinionCard[7];
}
