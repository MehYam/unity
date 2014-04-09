using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST.Game
{
    public interface IEffect
    {
        void Go(Game game, Hero hero);
    }

    public class MinionSpawner : IEffect
    {
        readonly int minionID;
        public MinionSpawner(int minionID)
        {
            this.minionID = minionID;
        }

        public void Go(Game g, Hero hero)
        {
            var gge = GlobalGameEvent.Instance;

            //KAI: never unsubscribes...
            gge.MinionPositionChosen += MinionPositionChosen;
            GlobalGameEvent.Instance.FireMinionPositionNeeded(hero);
        }

        void MinionPositionChosen(Hero h, int index)
        {
            GlobalGameEvent.Instance.MinionPositionChosen -= MinionPositionChosen;

            h.field.AddMinionAt(index, MinionFactory.Instance.CreateMinion(minionID));

            // choosing a minion's position ends the play of the card.
            GlobalGameEvent.Instance.FireCardPlayCompleted();
        }
    }

    public class TauntEffect : IEffect
    {
        public void Go(Game game, Hero hero)
        {
            throw new NotImplementedException();
        }
    }

    public class DivineShieldEffect : IEffect
    {
        public void Go(Game game, Hero hero)
        {
            throw new NotImplementedException();
        }
    }

}
