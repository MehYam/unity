using System.Collections.Generic;

public class Hero
{
    public int health { get; set; }
    public int mana { get; set; }
    public readonly Deck deck = new Deck();
    public Playfield field = new Playfield();
    public Hand hand = new Hand();

    public Hero()
    {
        health = 30;
        mana = 1;
    }
}

public class Playfield
{
    public readonly MinionCard[] minions = new MinionCard[7];
}

public class Hand
{
    public readonly AbstractAbility[] cards = new AbstractAbility[10];
}
