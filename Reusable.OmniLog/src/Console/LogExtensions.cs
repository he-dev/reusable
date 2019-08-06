using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Console
{
    public static class LogExtensions
    {
        public static LogEntry ConsoleTemplateBuilder(this LogEntry logEntry, ConsoleTemplateBuilder consoleTemplateBuilder)
        {
            return logEntry.SetItem(LogEntry.BasicPropertyNames.Message, nameof(ConsoleTemplateBuilder), consoleTemplateBuilder);
        }

        public static ConsoleTemplateBuilder ConsoleTemplateBuilder(this LogEntry logEntry)
        {
            return logEntry.GetItemOrDefault<ConsoleTemplateBuilder>(LogEntry.BasicPropertyNames.Message, nameof(ConsoleTemplateBuilder));
        }

        public static LogEntry ConsoleStyle(this LogEntry logEntry, ConsoleStyle? consoleStyle)
        {
            return logEntry.SetItem(LogEntry.BasicPropertyNames.Message, nameof(ConsoleStyle), consoleStyle);
        }

        public static ConsoleStyle? ConsoleStyle(this LogEntry logEntry)
        {
            return logEntry.GetItemOrDefault(LogEntry.BasicPropertyNames.Message, nameof(ConsoleStyle), default(ConsoleStyle?));
        }
    }
}