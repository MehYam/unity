using UnityEngine;
using System.Collections.Generic;

//KAI: this attempt is more interesting, but this is getting complicated - going to start with a very simple Card3 first, then move to something cleverer later
public sealed class Card2
{
    public interface ICardAttribute { }
    public abstract class IntCardAttribute { }
    public class Health : IntCardAttribute { }
    public class ManaCost : IntCardAttribute { }
    public class Overload : IntCardAttribute { }

    public class ChooseOne : ICardAttribute { }
    public class BattleCry : ICardAttribute { }
    public class Taunt : ICardAttribute { }
    public class Deathrattle : ICardAttribute { }

    private Dictionary<string, ICardAttribute> _attrs;
    public readonly string name;

    public Dictionary<string, ICardAttribute> CardAttributes
    {
        get
        {
            if (_attrs == null)
            {
                _attrs = new Dictionary<string, ICardAttribute>();
            }
            return _attrs;
        }
        private set {}
    }

    private Card2()
    {
        name = "unknown";
    }
}
