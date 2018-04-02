using System;
using Reusable.OmniLog.Collections;
using System.Linq;
using System.Linq.Custom;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.Utilities;

namespace Reusable.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var foo = new int[0];
            var bar = foo.Append(2);

            //Demo.ConsoleColorizer();
            Demo.SemanticExtensions();

            var rxFilter = new AppConfigRxFilter(NLogRx.Create());
            var loggerFactory = LoggerFactorySetup.SetupLoggerFactory("development", "VaultNET", rxFilter);

            Demo.DebuggerDisplay();

            System.Console.ReadKey();
        }
    }
}