using Reusable.OmniLog.Collections;

namespace Reusable.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Demo.ConsoleColorizer();
            Demo.SemLog();
            
            //System.Console.ReadKey();
        }

        private static void foo(ref readonly int x)
        {
            
        }
    }

    public static class Event
    {
        public const string ApplicationStart = nameof(ApplicationStart);
        public const string ApplicationExit = nameof(ApplicationExit);
        public const string InitializeConfiguration = nameof(InitializeConfiguration);
        public const string InitializeContainer = nameof(InitializeContainer);
    }
}