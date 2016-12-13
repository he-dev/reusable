using System;
using System.Collections.Generic;

namespace Reusable.Shelly
{
    public static class CommandLineExtensions
    {
        public static void ExecuteAndExit(this CommandLine commandLine, IEnumerable<string> args)
        {
            var exitCode = commandLine.Execute(args);
            Environment.Exit(exitCode);
        }
    }
}
