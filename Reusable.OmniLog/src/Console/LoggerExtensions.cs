using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Console
{
    public static class LoggerExtensions
    {
        public static void Write(this ILogger console, ConsoleStyle style, params ConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(new CompositeConsoleTemplateBuilder
            {
                IsParagraph = false,
                Style = style,
                Builders = builders
            }));
        }

        public static void WriteLine(this ILogger console, ConsoleStyle style, params ConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(new CompositeConsoleTemplateBuilder
            {
                IsParagraph = true,
                Style = style,
                Builders = builders
            }));
        }
    }
}