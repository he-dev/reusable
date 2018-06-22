using System.Linq.Custom;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.Utilities;

namespace Reusable.Apps
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var foo = new int[0];
            var bar = foo.Append(2);

            Demo.ConsoleColorizer();
            //Demo.SemanticExtensions();

            var rxFilter = new AppConfigRxFilter(NLogRx.Create());

            var loggerFactory =
                new LoggerFactoryBuilder()
                    .Environment("development")
                    .Product("Reusable.Apps.Console")
                    .WithRxes(rxFilter)
                    .Build();

            Demo.DebuggerDisplay();

            System.Console.ReadKey();
        }
    }
}