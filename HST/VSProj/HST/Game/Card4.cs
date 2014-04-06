using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HST.Game
{
    public class Card4
    {
        public readonly int id;

        public readonly string name;
        public readonly string text;
        public readonly int cost;
        public readonly ReadOnlyCollection<IEffect> effects;

        public Card4(int id, string name, string text, int cost, IList<IEffect> effects)
        {
            this.id = id;
            this.name = name;
            this.text = text;
            this.cost = cost;

            this.effects = new ReadOnlyCollection<IEffect>(effects);
        }

        public void Play(Game g)
        {
            foreach (var effect in effects)
            {
                effect.Go(g, g.turnHero);
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
        readonly Random _rnd = new Random();
        int _instances = 0;
        public Card4 CreateAbility(int id)
        {
            return null;
        }

        public Card4 CreateMinionCard(MinionFactory.MinionID id)
        {
            var effects = new IEffect[] { null };
            var card = new Card4(_instances++, "a card", "text", _rnd.Next(0, 11), effects);

            effects[0] = new MinionSpawner(card, id);
            return card;
        }

        public Card4 CreateCoin()
        {
            return new Card4(2112, "Coin", "Adds a mana", 0, new IEffect[] {});
        }
    }

}
