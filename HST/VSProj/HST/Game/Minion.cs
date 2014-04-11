using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HST.Game
{
    public sealed class Minion : ICharacter
    {
        public readonly string name;
        public bool awake { get; private set; }

        public bool taunt { get; set; }
        public bool stealth { get; set; }
        public bool charge { get; set; }
        public bool divineShield { get; set; }
        public bool windfury { get; set; }

        public readonly IList<IEffect> effects;

        public Minion(string name, int attack, int health, IList<IEffect> effects)
        {
            this.name = name;
            this.atk = attack;
            this.health = health;
            this.effects = effects;
            awake = false;
        }

        #region IDamageGiver, IDamageTaker, ICharacter
        public int atk { get; private set; }
        public int health { get; private set; }
        public bool canAttack
        {
            get
            {
                return awake && atk > 0 && !frozen;
            }
        }
        public bool frozen
        {
            get;
            set;
        }
        public void ReceiveAttack(int dmg)
        {
            health -= dmg;
            Logger.Log(string.Format("{0} receiving attack of {1} health->{2}", name, dmg, health));
        }
        #endregion

        //KAI: VISIBILITY - only called from Hero
        public void OnNewTurn(Game game)
        {
            awake = true;
        }

        public override string ToString()
        {
            return string.Format("Minion: {0}, atk {1}, hp {2}{3}", name, atk, health, awake ? "" : " ZZZ ");
        }

        public Minion Clone()
        {
            var clone = new Minion(name, atk, health, new List<IEffect>(effects));
            clone.taunt = this.taunt;
            clone.stealth = this.stealth;
            clone.charge = this.charge;
            clone.divineShield = this.divineShield;
            clone.windfury = this.windfury;

            return clone;
        }
    }
}
