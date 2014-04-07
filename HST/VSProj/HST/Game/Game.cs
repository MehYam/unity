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
        public Hero turnHero { get; private set; }
        public Hero turnDefender { get; private set; }
        public Game(Hero first, Hero second)
        {
            turnNumber = 0;

            _heros[0] = first;
            _heros[1] = second;

            //KAI: hokey - we don't need the heros array anymore
            turnHero = second;
            turnDefender = first;

            //KAI: references need to be cleaned up
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

            NextTurn();
        }

        public void NextTurn()
        {
            ++turnNumber;

            turnHero = _heros[(turnNumber - 1) % _heros.Length];
            turnDefender = _heros[turnNumber % _heros.Length];

            GlobalGameEvent.Instance.FireNewTurn(this);
        }

        public void Attack(ICharacter attacker, ICharacter attackee)
        {
            if (!(attacker is Hero) && !(attackee is Hero))
            {
                Logger.Log(string.Format("{0} attacks {1}", attacker, attackee));
            }
            attacker.ReceiveAttack(this, attackee); 
            attackee.ReceiveAttack(this, attacker);
        }

        static public void Attack(IEffect attacker, ICharacter attackee)
        {
            //KAI: attacker should maybe be IDamageGiver, but that's ambiguous with the above call
            throw new System.NotImplementedException();
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
