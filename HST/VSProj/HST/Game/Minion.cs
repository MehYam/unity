using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using HST.Util;

namespace HST.Game
{
    public sealed class Minion : ICharacter
    {
        public readonly string name;
        public int attacksLeft { get; private set; }

        public bool taunt { get; set; }
        public bool stealth { get; set; }
        public bool divineShield { get; set; }
        public bool windfury { get; private set; }
        public bool charge { get; private set; }

        public readonly IList<IEffect> effects;

        public event Action<Minion> Death = delegate { };

        public Minion(string name, int attack, int health, IList<IEffect> effects)
        {
            this.name = name;
            this.atk = attack;
            this.health = health;
            this.effects = effects;

            attacksLeft = 0; // minion starts life asleep, unless it's got charge
        }

        #region IDamageGiver, IDamageTaker, ICharacter
        public int atk { get; private set; }
        public int health { get; private set; }
        public bool canAttack
        {
            get
            {
                return attacksLeft > 0 && atk > 0 && !frozen;
            }
        }
        public bool frozen
        {
            get;
            set;
        }
        public void Attack(IDamageTaker victim)
        {
            DebugUtils.Assert(canAttack);

            --attacksLeft;

            victim.IncomingAttack(atk);

            // attacks are always counterattacked
            if (victim is IDamageGiver)
            {
                IncomingAttack(((IDamageGiver)victim).atk);
            }
        }
        public void IncomingAttack(int dmg)
        {
            if (divineShield)
            {
                divineShield = false;
                Logger.Log(name + " loses divine shield");
            }
            else
            {
                health -= dmg;
                Logger.Log(string.Format("{0} receiving attack of {1}, health->{2}", name, dmg, health));

                if (health <= 0)
                {
                    // event minion death
                    Death(this);
                }
            }
        }
        #endregion

        //KAI: overly visible (public) since only called from Hero?  Pass interfaces around?
        public void OnNewTurn(Game game)
        {
            attacksLeft = windfury ? 2 : 1;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (taunt) sb.Append("Taunt ");
            if (stealth) sb.Append("Stealth ");
            if (charge) sb.Append("Charge ");
            if (divineShield) sb.Append("Shield ");
            if (windfury) sb.Append("Windfury ");
            if (frozen) sb.Append("Frozen ");

            return string.Format("Minion: {0}, atk {1}, hp {2}, attacks {3}, {4}", name, atk, health, attacksLeft, sb.ToString());
        }

        public Minion Clone()
        {
            var effectsClone = effects == null ? null : new List<IEffect>(effects);
            var clone = new Minion(name, atk, health, effectsClone);
            clone.taunt = this.taunt;
            clone.stealth = this.stealth;
            clone.charge = this.charge;
            clone.divineShield = this.divineShield;
            clone.windfury = this.windfury;

            return clone;
        }
    }
}
