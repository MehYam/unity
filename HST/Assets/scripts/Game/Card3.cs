using System.Collections;

// this should be replaced by something a little more clever, once I figure out the design of cards
public sealed class Card3
{
    public enum Type { TOON, WEAPON, SPELL }

    public readonly Type type;
    public readonly long ID;

    public readonly string name;
    public readonly string text;
    public readonly int cost;
    public readonly bool deckable;

    Card3(Type type, string name, int cost, string text) 
    {
        this.type = type;
        this.ID = 0;

        this.name = name;
        this.cost = cost;
        this.text = text;
    }

    public Card3 Create(string JSON)
    {
        return new Card3(Type.TOON, "", 0, "");
    }

    // card events
    public interface CardEvents
    {
        void OnPlay();
        void OnAttack();
        void OnReceiveAttack();
        void OnDeath();
        void OnReturnToHand();
    }

    // implement the card effects here - these could be soft-coded in Lua instead, but this is good for starters
    public interface IScript { }

    // you can compose multiple effects together - something like a scripting language
    public sealed class CompositeScript : IScript { }

    public sealed class Taunt : IScript { }
    public sealed class DivineShield : IScript { }
    public sealed class Deathrattle : IScript { }
    public sealed class Battlecry : IScript { }
    public sealed class Frozen : IScript { }
    public sealed class Transform : IScript { }  // transformed into a different card/minion
    public sealed class OnReturnToHand : IScript { }
}

