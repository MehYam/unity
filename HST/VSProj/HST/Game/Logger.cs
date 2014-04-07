using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST.Game
{
    class Logger
    {
        static public ILogger Impl
        {
            set;
            private get;
        }
        static public void Log(string str)
        {
            Impl.Log(str);
        }

        static public void LogResult(string str, bool pass)
        {
            Impl.LogResult(str, pass);
        }
    }
}
