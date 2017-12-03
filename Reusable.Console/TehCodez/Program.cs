using System;
using Reusable.OmniLog.Collections;
using System.Linq;
using System.Linq.Custom;

namespace Reusable.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Demo.ConsoleColorizer();
            Demo.SemLog();

            var foo = new int[0];

            var bar = foo.Append(2);

            //System.Console.ReadKey();
        }

        //private static void foo(ref readonly int x)
        //{
            
        //}
    }

   
}