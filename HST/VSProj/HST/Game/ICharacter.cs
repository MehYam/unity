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
    }

    public interface IDamageTaker
    {
        int health { get; }
        void ReceiveAttack(int dmg);
    }

    public interface ICharacter : IDamageGiver, IDamageTaker
    {
        bool frozen { get; set; }
    }
}
