using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using HST.Util;

namespace HST.Game
{
    public class Card4
    {
        public enum Type { MINION = 4, SPELL = 5, WEAPON = 7, HERO_POWER = 10 };
        
        public readonly string id;
        public readonly Type type; // KAI: class hierarchy instead?

        public readonly string name;
        public readonly string text;
        public readonly int cost;
        public readonly ReadOnlyCollection<IEffect> effects;

        public Card4(string id, Type type, string name, string text, int cost, IList<IEffect> effects)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.text = text;
            this.cost = cost;

            this.effects = new ReadOnlyCollection<IEffect>(effects);
        }

        public void Play(Game g)
        {
            GlobalGameEvent.Instance.FireCardPlayStarted(g.turnHero, this);
            foreach (var effect in effects)
            {
                effect.Go(g, g.turnHero);
            }

            //KAI: weak... this needs to be rethought
            if (effects.Count == 0)
            {
                GlobalGameEvent.Instance.FireCardPlayCompleted();
            }
        }

        public override string ToString()
        {
            return string.Format("Card {3,3}: {0}, {1}, cost {2,2}", name, text, cost, id);
        }
    }

    public class ModdedCard<T> where T : Card4
    {
        // i.e. for when a card's cost been changed
        public readonly T ability;
        public readonly int modifiers;
    }

    //KAI: this factory does not yet hide the concrete types - maybe it shouldn't?
    public class CardFactory
    {
        CardFactory() { } // hide constructor

        static CardFactory _instance;
        static public CardFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardFactory();
                }
                return _instance;
            }
        }
        int _instances = 0;


        IList<Card4> _cards = new List<Card4>();
        Dictionary<string, Card4> _cardLookupByName = new Dictionary<string, Card4>();

        static string SafeGetValue(Hashtable node, string name)
        {
            object value = node[name];
            return value == null ? null : value.ToString();
        }
        static int SafeGetInt(Hashtable node, string name)
        {
            object value = node[name];
            return value == null ? 0 : int.Parse( value.ToString() );
        }
        static bool SafeGetBool(Hashtable node, string name)
        {
            var value = SafeGetValue(node, name);
            return value != null && value != "0" && string.Compare(value, "false", true) != 0;
        }
        public void LoadCards(string JSON)
        {
            var retval = MiniJsonExtensions.hashtableFromJson(JSON);

            var cardsNode = (ArrayList)MiniJsonExtensions.getNode(retval, "cards");
            foreach (Hashtable card in cardsNode)
            {
                // not a lot of error checking here, this is a file that's not apt to change
                // without a lot of testing first, so we'll play this fast and loose.
                var cardID  = SafeGetValue(card, "id");
                var name = SafeGetValue(card, "name");
                var cost = SafeGetInt(card, "cost");
                var cardType = (Card4.Type)SafeGetInt(card, "type");
                var text = SafeGetValue(card, "text");
                var targetingText = SafeGetValue(card, "targetingText");

                var atk = SafeGetInt(card, "atk");
                var freeze = SafeGetBool(card, "freeze");

                IList<IEffect> effects = new List<IEffect>();
                switch (cardType)
                {
                    case Card4.Type.MINION:
                        var health = SafeGetInt(card, "health");
                        var battlecry = SafeGetBool(card, "battlecry");
                        var taunt = SafeGetBool(card, "taunt");
                        var stealth = SafeGetBool(card, "stealth");
                        var charge = SafeGetBool(card, "charge");
                        var divineShield = SafeGetBool(card, "divineShield");
                        var windfury = SafeGetBool(card, "windfury");
                        var combo = SafeGetBool(card, "combo");
                        var overload = SafeGetBool(card, "overload");
                        var spellpower = SafeGetBool(card, "spellpower");

                        //KAI: something here is redundant... 
                        //...we have MinionSpawner, MinionType, MinionFactory
                        int minionID = MinionFactory.Instance.AddMinionType(name, atk, health);

                        effects.Add(new MinionSpawner(minionID));
                        break;
                    case Card4.Type.SPELL:
                        //KAI: the card xml has "ReferencedTag", something we should possibly use here
                        break;
                    case Card4.Type.WEAPON:
                        var durability = SafeGetInt(card, "durability");
                        break;
                }

                _cards.Add(new Card4(cardID, cardType, name, text, cost, effects));
            }
        }

        public int NumCards { get { return _cards.Count; } }
        public Card4 GetCard(int index)
        {
            return _cards[index];
        }
        public Card4 GetCard(string cardName)
        {
            return _cardLookupByName[cardName];
        }

        public Card4 CreateCoin()
        {
            return new Card4("Fake Coin", Card4.Type.SPELL, "Coin", "Adds a mana", 0, new IEffect[] {});
        }
    }

}
