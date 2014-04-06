using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST
{
    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Tests(new ConsoleLogger());
            tests.RunAll();

            Console.ReadKey(true);
        }
    }
}
