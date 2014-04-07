using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HST.Game
{
    public sealed class Minion : ICharacter
    {
        readonly Card4 card;

        public int atk { get; private set; }
        public int health { get; private set; }

        public bool awake { get; private set; }
        public readonly IList<IEffect> effects;

        public Minion(Card4 spawner, int attack, int health, IList<IEffect> effects)
        {
            this.card = spawner;
            this.atk = attack;
            this.health = health;
            this.effects = effects;
            awake = false;
        }

        public void ReceiveAttack(Game g, IDamageGiver attacker)
        {
            Logger.Log(string.Format("{0} receiving attack of {1}", this, attacker.atk));

            //KAI: here we need to loop effects first
            health -= attacker.atk;

            if (health <= 0)
            {
                Logger.Log(string.Format("{0} has been KILLED!", this));

                GlobalGameEvent.Instance.FireMinionDeath(g, this);
            }
        }

        public void OnNewTurn(Game game)
        {
            awake = true;
        }

        public override string ToString()
        {
            return string.Format("Minion: {0}, atk {1}, hp {2}, awake {3}", card, atk, health, awake);
        }
    }

    public sealed class MinionFactory
    {
        static MinionFactory _instance;
        static public MinionFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MinionFactory();
                }
                return _instance;
            }
        }

        public enum MinionID
        {
            YETI,
            CROCOLISK,
            WISP,
            BOULDERFIST,

            MAX   // maximum, not a minion named MAX, although there should be a minion named MAX.
        }
        readonly Dictionary<MinionID, Func<Minion>> constructors = new Dictionary<MinionID, Func<Minion>>();
        public Minion CreateMinion(MinionID id)
        {
            Func<Minion> constructor = null;
            if (constructors.TryGetValue(id, out constructor))
            {
                return constructor.Invoke();
            }
            return null;
        }

        MinionFactory() 
        {
            constructors[MinionID.YETI] = () => new Minion(null, 4, 5, null);
            constructors[MinionID.CROCOLISK] = () => new Minion(null, 2, 3, null);
            constructors[MinionID.WISP] = () => new Minion(null, 1, 1, null);
            constructors[MinionID.BOULDERFIST] = () => new Minion(null, 6, 7, null);
        }
    }
}
