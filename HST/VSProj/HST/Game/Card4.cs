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
            return string.Format("{0,20},{1,2},{3,8}, {2}", name, cost, text, id);
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

        IList<Card4> _cards = new List<Card4>();
        Dictionary<string, Card4> _cardLookupByName = new Dictionary<string, Card4>();
        Dictionary<string, Card4> _cardLookupByFileID = new Dictionary<string, Card4>();

        public void LoadCards(string jsonText)
        {
            var json = MJSON.hashtableFromJson(jsonText);

            var cardsNode = (ArrayList)MJSON.getNode(json, "cards");
            foreach (Hashtable card in cardsNode)
            {
                // not a lot of error checking here, this is a file that's not apt to change
                // without a lot of testing first, so we'll play this fast and loose.
                var cardID  = MJSON.SafeGetValue(card, "id");
                var name = MJSON.SafeGetValue(card, "name");
                var cost = MJSON.SafeGetInt(card, "cost");
                var cardType = (Card4.Type)MJSON.SafeGetInt(card, "type");
                var text = MJSON.SafeGetValue(card, "text");
                var targetingText = MJSON.SafeGetValue(card, "targetingText");

                var atk = MJSON.SafeGetInt(card, "atk");
                var freeze = MJSON.SafeGetBool(card, "freeze");

                IList<IEffect> effects = new List<IEffect>();
                switch (cardType)
                {
                    case Card4.Type.MINION:
                        var health = MJSON.SafeGetInt(card, "health");
                        var battlecry = MJSON.SafeGetBool(card, "battlecry");
                        var taunt = MJSON.SafeGetBool(card, "taunt");
                        var stealth = MJSON.SafeGetBool(card, "stealth");
                        var charge = MJSON.SafeGetBool(card, "charge");
                        var divineShield = MJSON.SafeGetBool(card, "divineShield");
                        var windfury = MJSON.SafeGetBool(card, "windfury");
                        var combo = MJSON.SafeGetBool(card, "combo");
                        var overload = MJSON.SafeGetBool(card, "overload");
                        var spellpower = MJSON.SafeGetBool(card, "spellpower");

                        //KAI: something here is redundant... 
                        //...we have MinionSpawner, MinionType, MinionFactory
                        int minionID = MinionFactory.Instance.AddMinionType(name, atk, health);

                        effects.Add(new MinionSpawner(minionID));
                        break;
                    case Card4.Type.SPELL:
                        //KAI: the card xml has "ReferencedTag", something we should possibly use here
                        var secret = MJSON.SafeGetBool(card, "secret");
                        break;
                    case Card4.Type.WEAPON:
                        var durability = MJSON.SafeGetInt(card, "durability");
                        break;
                }

                var newCard = new Card4(cardID, cardType, name, text, cost, effects);
                _cards.Add(newCard);
                _cardLookupByName[name] = newCard;
                _cardLookupByFileID[cardID] = newCard;
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
