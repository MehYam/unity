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
        void Attack(Game game, IDamageTaker victim);
    }

    public interface IDamageTaker
    {
        int health { get; }
        void ReceiveAttack(Game game, IDamageGiver attacker);
    }

    public interface ICharacter : IDamageGiver, IDamageTaker    {}
}
