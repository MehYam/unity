using System.Collections;


//KAI: this was my first attempt at a design for cards - kind of a standard object-oriented hierarchy.  I want to move to a more
// composition and behavior-based approach, which is what Card2 is.
public abstract class Card
{
    public enum Type { TOON, WEAPON, SPELL }

    public readonly string name;
    public readonly int cost;
    public readonly string text;
    public readonly bool hasCastTarget;      // for battlecries, spells
    public int costModifier    { get; set; }


    public static Card Create()
    {
        // create a card from JSON
        return null;
    }

    protected Card()
    {
        name = "unknown";
        cost = 0;
        text = "This card is UNREAL";
    }


}
/*
public class MinionCard : Card
{
    public bool battlecry { get; set; }
    public bool taunt { get; set; }
    public bool wasSilenced { get; set; }
    public bool deathRattle { get; set; }
    public bool chooseOne { get; set; }
    public int  overload { get; set; }

    private MinionCard()
    {
    }
}

public class WeaponCard : Card
{
    private WeaponCard()
    {
    }
}

public class SpellCard : Card
{
    private SpellCard()
    {
    }
}
*/
