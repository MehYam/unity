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
        readonly Card4 card;
        readonly MinionFactory.MinionID minionID;
        public MinionSpawner(Card4 card, MinionFactory.MinionID ID)
        {
            this.card = card;
            minionID = ID;

            System.Console.WriteLine(string.Format("spawner card id {0}, minion {1}", card.id, minionID));
        }

        public void Go(Game g, Hero hero)
        {
            var gge = GlobalGameEvent.Instance;

            //KAI: this never unsubscribes...
            gge.MinionPositionChosen += MinionPositionChosen;
            GlobalGameEvent.Instance.FireMinionPositionNeeded(hero);
        }

        void MinionPositionChosen(Hero h, int index)
        {
            GlobalGameEvent.Instance.MinionPositionChosen -= MinionPositionChosen;

            h.field.AddMinionAt(index, MinionFactory.Instance.CreateMinion(minionID));

            // choosing a minion's position ends the play of the card.
            GlobalGameEvent.Instance.FireCardPlayCompleted(h, card);
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
