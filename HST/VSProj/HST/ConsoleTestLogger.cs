using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST
{
    sealed class ConsoleTestLogger : ITestLogger
    {
        public void Log(string name, string msg)
        {
            Console.WriteLine(string.Format("{0} => {1}", name, msg));
        }
        public void LogResult(string name, bool pass, string msg = null)
        {
            Console.WriteLine(string.Format("{0} => {1} {2}", name, pass ? "pass" : "FAILED", msg == null ? "" : msg));
        }
    }
}
