using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HST.Util;

namespace HST.Game
{
    public class Playfield : IEnumerable<Minion>
    {
        static public readonly int MAX_MINIONS = 7;
        readonly LinkedList<Minion> minions = new LinkedList<Minion>();

        public int size { get { return minions.Count; } }
        public Minion this[int index]
        {
            get
            {
                return minions.ElementAt(index);
            }
        }

        public void AddMinion(Minion minion)
        {
            AddMinionAt(0, minion);
        }

        // note, minions are "centered" at 0, so the indices run from -3 to +3.  Caller
        // can actually safely pass in any index.
        public void AddMinionAt(int indexFromCenter, Minion minion)
        {
            if (minions.Count < MAX_MINIONS)
            {
                var centerIndex = minions.Count / 2;
                var insertIndex = centerIndex + indexFromCenter;
                if (insertIndex <= 0)
                {
                    minions.AddFirst(minion);
                }
                else
                {
                    if (insertIndex >= minions.Count)
                    {
                        insertIndex = minions.Count - 1;
                    }

                    var node = minions.First;
                    for (var i = 0; i < insertIndex; ++i)
                    {
                        node = node.Next;
                    }
                    minions.AddAfter(node, minion);
                }

                minion.Death += OnMinionDeath;
            }
            else
            {
                DebugUtils.Assert(false);
            }
        }

        public bool RemoveMinion(Minion minion)
        {
            minion.Death -= OnMinionDeath;
            return minions.Remove(minion);
        }

        #region events
        public void OnNewTurn(Game game)
        {
            if (game.turnHero.field == this)
            {
                foreach (var minion in this)
                {
                    minion.OnNewTurn(game);
                }
            }
        }
        void OnMinionDeath(Minion minion)
        {
            Logger.Log(string.Format("{0} dies, removing from playfield", minion));
            RemoveMinion(minion);
        }
        #endregion

        public IEnumerator<Minion> GetEnumerator()
        {
            return minions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return minions.GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var minion in minions)
            {
                sb.AppendLine(minion.ToString());
            }

            return sb.ToString();
        }
    }
}
