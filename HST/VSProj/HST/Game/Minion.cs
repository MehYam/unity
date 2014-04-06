using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HST.Game
{
    public sealed class Minion
    {
        readonly Card4 card;

        public int attack { get; private set; }
        public int health { get; private set; }
        public readonly IList<IEffect> effects;

        public Minion(Card4 spawner, int attack, int health, IList<IEffect> effects)
        {
            this.card = spawner;
            this.attack = attack;
            this.health = health;
            this.effects = effects;
        }

        public override string ToString()
        {
            return string.Format("Minion: {0}, atk {1}, hp {2}", card, attack, health);
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
