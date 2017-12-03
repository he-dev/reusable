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
            var foo = new int[0];
            var bar = foo.Append(2);

            Demo.ConsoleColorizer();
            Demo.SemLog();


            //System.Console.ReadKey();
        }

        //private static void foo(ref readonly int x)
        //{
            
        //}
    }
}