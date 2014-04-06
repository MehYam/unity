using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST
{
    sealed class ConsoleLogger : ILogger
    {
        public void Log(string str)
        {
            Console.WriteLine("=>" + str);
        }
        public void LogResult(string str, bool pass)
        {
            var result = pass ? "pass" : "FAILED";
            Console.WriteLine(string.Format("{0} => {1}", result, str));
        }
    }
}
