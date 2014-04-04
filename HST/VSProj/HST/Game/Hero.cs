using System.Collections.Generic;

public class Hero
{
    public int health { get; set; }
    public int mana { get; set; }
    public readonly Deck deck = new Deck();
    public Playfield field = new Playfield();
    public Hand hand = new Hand();

    Hero()
    {
        health = 30;
        mana = 1;
    }

    public enum CLASS { MAGE, WARRIOR, PRIEST, PALADIN, WARLOCK, DRUID, HUNTER, SHAMAN, ROGUE };
    static public Hero CreateHero(CLASS heroClass)
    {
        return new Hero();
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

    public void Draw(int cards)
    {
        for (int i = 0; i < cards; ++i)
        {
            var card = deck.Draw();
            hand.AddCard(card);
        }
    }
}

public class Playfield
{
    public readonly MinionCard[] minions = new MinionCard[7];
}
