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
        public readonly LinkedList<Minion> minions = new LinkedList<Minion>();

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
            }
            else
            {
                DebugUtils.Assert(false);
            }
        }

        public void RemoveMinion(Minion minion)
        {
            minions.Remove(minion);
        }

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
