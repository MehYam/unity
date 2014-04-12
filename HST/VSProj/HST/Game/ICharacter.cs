using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST.Game
{
    public interface IDamageGiver
    {
        int atk { get; }
        bool canAttack { get; }

        void Attack(IDamageTaker victim);
    }

    public interface IDamageTaker
    {
        int health { get; }

        void IncomingAttack(int dmg);
    }

    public interface ICharacter : IDamageGiver, IDamageTaker
    {
        bool frozen { get; set; }
    }
}
