using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HST.Game
{
    public sealed class Minion : ICharacter
    {
        public readonly string name;
        public int atk { get; private set; }
        public int health { get; private set; }

        public bool awake { get; private set; }
        public readonly IList<IEffect> effects;

        public Minion(string name, int attack, int health, IList<IEffect> effects)
        {
            this.name = name;
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
            return string.Format("Minion: {0}, atk {1}, hp {2}{3}", name, atk, health, awake ? "" : " ZZZ ");
        }
    }

    public sealed class MinionFactory
    {
        MinionFactory() { }
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

        class MinionType
        {
            public readonly string name;
            public readonly int atk;
            public readonly int health;

            public MinionType(string name, int atk, int health)
            {
                this.name = name;
                this.atk = atk;
                this.health = health;
            }
        }
        readonly IList<MinionType> minionTypes = new List<MinionType>();

        /// <summary>
        /// Returns the ID associated with the minion
        /// </summary>
        /// <returns></returns>
        public int AddMinionType(string name, int atk, int health)
        {
            var minionID = minionTypes.Count;
            minionTypes.Add(new MinionType(name, atk, health));

            return minionID;
        }

        public Minion CreateMinion(int id)
        {
            var spec = minionTypes[id];

            return new Minion(spec.name, spec.atk, spec.health, null);
        }
    }
}
