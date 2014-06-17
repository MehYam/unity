#define DEBUG

using UnityEngine;
using System.Collections;

namespace PvT.Util
{
    public static class DebugUtil
    {
        [System.Diagnostics.Conditional("DEBUG")]
        static public void Assert(bool condition)
        {
            if (!condition) throw new System.Exception();
        }
    }
}
