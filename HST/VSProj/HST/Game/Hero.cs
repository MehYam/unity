using System.Collections.Generic;
using HST.Util;

namespace HST.Game
{
    public sealed class Hero : ICharacter
    {
        public enum CLASS { MAGE, WARRIOR, PRIEST, PALADIN, WARLOCK, DRUID, HUNTER, SHAMAN, ROGUE };
        public readonly CLASS heroClass;

        public int atk { get; private set; }
        public int health { get; private set; }
        public int mana { get; private set; }
        public int crystals { get; private set; }

        public Deck<Card4> deck { get; private set; }
        public Hand<Card4> hand { get; private set; }
        public Playfield field { get; private set; }

        static readonly int START_HEALTH = 30;
        static readonly int MAX_MANA = 10;
        Hero(CLASS heroClass)
        {
            this.heroClass = heroClass;

            health = START_HEALTH;
            mana = 1;
            crystals = 1;

            deck = new Deck<Card4>(Game.DECKSIZE);
            hand = new Hand<Card4>();
            field = new Playfield();

            //KAI: never unsubscribes...
            GlobalGameEvent.Instance.NewTurn += OnNewTurn;
            GlobalGameEvent.Instance.CardPlayCompleted += OnCardPlayCompleted;
        }

        void OnNewTurn(Game g)
        {
            if (g.turnHero == this)
            {
                crystals = System.Math.Min(mana + 1, MAX_MANA);
                mana = crystals;

                Draw(1);
            }
            field.OnNewTurn(g);
        }
        void OnCardPlayCompleted(Hero h, Card4 c)
        {
            if (h == this)
            {
                DebugUtils.Assert(c.cost <= mana);

                crystals -= c.cost;
            }
        }

        static public Hero CreateHero(CLASS heroClass)
        {
            return new Hero(heroClass);
        }

        public void Draw(int cards)
        {
            for (int i = 0; i < cards; ++i)
            {
                var card = deck.Draw();
                hand.AddCard(card);
            }
        }

        public void Mulligan(Hand<Card4> cardsToMully)
        {
            DebugUtils.Assert(cardsToMully.size < Game.DECKSIZE);

            // rebuild a new deck with the undrawn and mully'ed cards, then shuffle it
            var newDeck = new Deck<Card4>(Game.DECKSIZE - hand.size + cardsToMully.size);
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

            DebugUtils.Assert(
                (newDeckIndex + hand.size) == Game.DECKSIZE &&
                newDeck.remaining + hand.size == Game.DECKSIZE);

            deck = newDeck;
        }

        public void ReceiveAttack(IDamageGiver attacker)
        {
            this.health -= attacker.atk;

            Logger.Log(string.Format("{0} receiving attack from {1}, new health {2}",heroClass, attacker, health));
        }

        public void ReceiveAttack(IEffect effect)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("{0}, {1} hp, {2}/{3} mana", heroClass.ToString(), health, mana, crystals));
            sb.AppendLine("Deck cards remaining: " + deck.remaining);
            sb.AppendLine(deck.ToString());
            sb.AppendLine("Hand:");
            sb.AppendLine(hand.ToString());
            sb.AppendLine("Playfield:");
            sb.AppendLine(field.ToString());

            return sb.ToString();
        }
        public string ToStringBrief()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("{0}, {1} hp, {2}/{3} mana", heroClass.ToString(), health, mana, crystals));
            sb.AppendLine("Deck cards remaining: " + deck.remaining);
            sb.AppendLine("Hand:");
            sb.AppendLine(hand.ToString());
            sb.AppendLine("Playfield:");
            sb.AppendLine(field.ToString());

            return sb.ToString();
        }
    }
}
