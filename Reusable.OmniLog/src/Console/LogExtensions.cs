using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Console
{
    public static class LogExtensions
    {
        //private static readonly string FallbackTemplate = HtmlElement.Builder.p(p => p.text("{text}")).ToHtml();

        public static ILog ConsoleTemplateBuilder(this ILog log, ConsoleTemplateBuilder consoleTemplateBuilder)
        {
            return log.SetItem(nameof(ConsoleTemplateBuilder), consoleTemplateBuilder);
        }

        public static ConsoleTemplateBuilder ConsoleTemplateBuilder(this ILog log)
        {
            return log.GetItemOrDefault<ConsoleTemplateBuilder>(nameof(ConsoleTemplateBuilder));
        }
    }
}