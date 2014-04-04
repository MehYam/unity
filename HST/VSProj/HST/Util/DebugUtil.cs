#define DEBUG

using System;
using System.Diagnostics;

namespace HST.Util
{
    public static class DebugUtils
    {
        [Conditional("DEBUG")]
        static public void Assert(bool condition)
        {
            if (!condition) throw new Exception();
        }
    }
}
