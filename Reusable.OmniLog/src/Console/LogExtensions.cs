using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Console
{
    public static class LogExtensions
    {
        public static ILog ConsoleTemplateBuilder(this ILog log, ConsoleTemplateBuilder consoleTemplateBuilder)
        {
            return log.SetItem(nameof(ConsoleTemplateBuilder), consoleTemplateBuilder);
        }

        public static ConsoleTemplateBuilder ConsoleTemplateBuilder(this ILog log)
        {
            return log.GetItemOrDefault<ConsoleTemplateBuilder>(nameof(ConsoleTemplateBuilder));
        }
        
        public static ILog ConsoleStyle(this ILog log, ConsoleStyle? consoleStyle)
        {
            return log.SetItem(nameof(ConsoleStyle), consoleStyle);
        }

        public static ConsoleStyle? ConsoleStyle(this ILog log)
        {
            return log.GetItemOrDefault(nameof(ConsoleStyle), default(ConsoleStyle?));
        }
    }
}