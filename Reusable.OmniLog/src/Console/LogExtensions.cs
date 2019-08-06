using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Console
{
    public static class LogExtensions
    {
        public static Log ConsoleTemplateBuilder(this Log log, ConsoleTemplateBuilder consoleTemplateBuilder)
        {
            return log.SetItem(Log.PropertyNames.Message, nameof(ConsoleTemplateBuilder), consoleTemplateBuilder);
        }

        public static ConsoleTemplateBuilder ConsoleTemplateBuilder(this Log log)
        {
            return log.GetItemOrDefault<ConsoleTemplateBuilder>(Log.PropertyNames.Message, nameof(ConsoleTemplateBuilder));
        }

        public static Log ConsoleStyle(this Log log, ConsoleStyle? consoleStyle)
        {
            return log.SetItem(Log.PropertyNames.Message, nameof(ConsoleStyle), consoleStyle);
        }

        public static ConsoleStyle? ConsoleStyle(this Log log)
        {
            return log.GetItemOrDefault(Log.PropertyNames.Message, nameof(ConsoleStyle), default(ConsoleStyle?));
        }
    }
}