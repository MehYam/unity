using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST.Game
{
    public interface IDamageGiver
    {
        int atk { get; }
    }

    public interface IDamageTaker
    {
        int health { get; }
    }

    public interface ICharacter : IDamageGiver, IDamageTaker
    {
        void ReceiveAttack(IDamageGiver attacker);
    }
}
