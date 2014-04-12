using System.Collections.Generic;
using HST.Util;

namespace HST.Game
{
    public sealed class Hero : ICharacter
    {
        public enum CLASS { MAGE, WARRIOR, PRIEST, PALADIN, WARLOCK, DRUID, HUNTER, SHAMAN, ROGUE };
        public readonly CLASS heroClass;

        public int mana { get; private set; }
        public int crystals { get; set; }

        public Deck<AbstractCard> deck { get; private set; }
        public Hand<AbstractCard> hand { get; private set; }
        public Playfield field { get; private set; }

        static readonly int START_HEALTH = 30;
        static readonly int MAX_MANA = 10;
        Hero(CLASS heroClass)
        {
            this.heroClass = heroClass;

            health = START_HEALTH;
            mana = 0;
            crystals = 0;

            deck = new Deck<AbstractCard>(Game.DECKSIZE);
            hand = new Hand<AbstractCard>();
            field = new Playfield();

            GlobalGameEvent.Instance.CardPlayComplete += OnCardPlayComplete;
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

        public void Mulligan(Hand<AbstractCard> cardsToMully)
        {
            DebugUtils.Assert(cardsToMully.size < Game.DECKSIZE);

            // rebuild a new deck with the undrawn and mully'ed cards, then shuffle it
            var newDeck = new Deck<AbstractCard>(Game.DECKSIZE - hand.size + cardsToMully.size);
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

        #region ICharacter implementation
        public int atk { get; private set; }
        public int health { get; private set; }
        public bool frozen { get; set; }
        public bool canAttack { get { return false; } } // not yet
        public void Attack(IDamageTaker taker)
        {
            throw new System.NotImplementedException();
        }
        public void IncomingAttack(int dmg)
        {
            this.health -= dmg;

            Logger.Log(string.Format("{0} receiving attack of {1} health->{2}", heroClass, dmg, health));
        }
        #endregion

        #region events
        public void OnNewTurn(Game g)
        {
            if (g.turnHero == this)
            {
                crystals = System.Math.Min(crystals + 1, MAX_MANA);
                mana = crystals;

                Draw(1);
            }
            field.OnNewTurn(g);
        }
        public void OnCardPlayComplete(Game g, AbstractCard card)
        {
            if (g.turnHero == this)
            {
                DebugUtils.Assert(mana >= card.cost);

                mana -= card.cost;
                hand.PullCard(card);
            }
        }
        #endregion

        public override string ToString()
        {
            return ToStringImpl();
        }
        public string ToStringBrief()
        {
            return ToStringImpl(true);
        }
        string ToStringImpl(bool brief = false)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("{0}, {1} hp, {2}/{3} mana", heroClass.ToString(), health, mana, crystals));
            sb.AppendLine("Deck cards remaining: " + deck.remaining);
            if (!brief)
            {
                sb.AppendLine(deck.ToString());
            }
            sb.AppendLine("--Hand--");
            sb.Append(hand.ToString());
            if (field.size > 0)
            {
                sb.AppendLine();
                sb.AppendLine("--Playfield--");
                sb.Append(field.ToString());
            }
            return sb.ToString();
        }
    }
}
