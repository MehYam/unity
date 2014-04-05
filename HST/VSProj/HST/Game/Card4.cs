using System;

namespace HST.Game
{
    public abstract class AbstractCard
    {
        public readonly int id;

        public readonly string name;
        public readonly string text;
        public readonly int cost;

        public AbstractCard(int id, string name, string text, int cost)
        {
            this.id = id;
            this.name = name;
            this.text = text;
            this.cost = cost;
        }

        public abstract void Play(Game g, Hero hero);

        public override string ToString()
        {
            return string.Format("Card {3,3}: {0}, {1}, cost {2,2}", name, text, cost, id);
        }
    }

    public sealed class MinionCard : AbstractCard
    {
        public readonly int attack;
        public readonly int health;
        public MinionCard(int id, string name, string text, int cost, int attack, int health)
            : base(id, name, text, cost)
        {
            this.attack = attack;
            this.health = health;
        }

        public override void Play(Game g, Hero hero)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("{0}, attack {1,2}, health {2,2}", base.ToString(), attack, health);
        }
    }

    public class SpellCard : AbstractCard
    {
        public SpellCard(int id, string name, string text, int cost)
            : base(id, name, text, cost)
        {
        }

        public override void Play(Game g, Hero hero)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("{0} (spell)", base.ToString());
        }
    }

    // cards are immutable, we attach modifiers to affect them
    public class PlayedCard<T> where T : AbstractCard
    {
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
        public AbstractCard CreateAbility(int id)
        {
            return null;
        }

        public MinionCard CreateMinionCard(int id)
        {
            return CreateRandomMinionCard();
        }
        public MinionCard CreateRandomMinionCard()
        {
            return new MinionCard(++_instances, "minion", "minion", _rnd.Next(10), _rnd.Next(12), _rnd.Next(1, 12));
        }

        public SpellCard CreateCoin()
        {
            return new SpellCard(2112, "Coin", "Adds a mana", 0);
        }
    }

}
