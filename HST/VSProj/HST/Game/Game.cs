using System.Collections;

using HST.Util;

namespace HST.Game
{
    public class Game
    {
        static public int DECKSIZE = 30;  //KAI: non-readonly for testing purposes, got to be a nicer way
        static public readonly int INITIAL_DRAW = 3;
        readonly Hero[] _heros = new Hero[2];
        public Hero[] heros
        {
            get
            {
                return _heros;
            }
        }

        public int turnNumber { get; private set; }
        public Hero turnHero
        {
            get
            {
                return _heros[(turnNumber - 1) % _heros.Length];
            }
        }
        public Game(Hero first, Hero second)
        {
            turnNumber = 1;

            _heros[0] = first;
            _heros[1] = second;

            //KAI: needs to be cleaned up
            GlobalGameEvent.Instance.CardPlayCompleted += OnCardPlayCompleted;
        }

        public void DrawForMulligan()
        {
            _heros[0].deck.Shuffle();
            _heros[1].deck.Shuffle();

            _heros[0].Draw(INITIAL_DRAW);
            _heros[1].Draw(INITIAL_DRAW + 1);
        }

        public void OnPostMulligan()
        {
            _heros[0].Draw(INITIAL_DRAW - _heros[0].hand.size);
            _heros[1].Draw((INITIAL_DRAW + 1) - _heros[1].hand.size);

            _heros[1].hand.AddCard(CardFactory.Instance.CreateCoin());

            DebugUtils.Assert(_heros[0].hand.size == Game.INITIAL_DRAW);
            DebugUtils.Assert(_heros[1].hand.size == Game.INITIAL_DRAW + 2);
        }

        public void NextTurn()
        {
            ++turnNumber;
            GlobalGameEvent.Instance.FireNewTurn();
        }

        static void OnCardPlayCompleted(Hero h, Card4 card)
        {
            //KAI: theoretically only the hand needs to subscribe to this?
            h.hand.PullCard(card);
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(_heros[0].ToString());
            sb.AppendLine(_heros[1].ToString());

            return sb.ToString();
        }
        public string ToStringBrief()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(_heros[0].ToStringBrief());
            sb.AppendLine(_heros[1].ToStringBrief());

            return sb.ToString();
        }
    }
}
