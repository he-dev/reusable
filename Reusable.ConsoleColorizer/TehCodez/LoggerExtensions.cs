using System;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

namespace Reusable.ConsoleColorizer
{
    public static class LoggerExtensions
    {
        public static ILogger ConsoleMessage(this ILogger logger, Func<HtmlElement, HtmlElement> paragraphAction)
        {
            var html = paragraphAction(HtmlElement.Builder.span()).ToHtml(HtmlFormatting.Empty);

            return logger.Log(LogLevel.Information, log => log.With(nameof(ConsoleMessage), html));
        }
        
        public static ILogger ConsoleMessageLine(this ILogger logger, Func<HtmlElement, HtmlElement> paragraphAction)
        {
            var html = paragraphAction(HtmlElement.Builder.p()).ToHtml(HtmlFormatting.Empty);

            return logger.Log(LogLevel.Information, log => log.With(nameof(ConsoleMessage), html));
        }
        
//        public static ILogger ConsoleParagraph(this ILogger logger, Action<ConsoleTemplate> paragraphAction)
//        {
//            var paragraph = ConsoleTemplate.Builder.ConsoleParagraph();
//            paragraphAction(paragraph);
//
//            return logger.Log(LogLevel.Information, log => log.With(nameof(ConsoleTemplate), paragraph.ToConsoleTemplate()));
//        }
//        
//        public static ILogger ConsoleSpan(this ILogger logger, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, params Func<ConsoleTemplate, object>[] content)
//        {
//            var span = ConsoleTemplate.Builder.ConsoleSpan(foregroundColor, backgroundColor, content);
//
//            return logger.Log(LogLevel.Information, log => log.With(nameof(ConsoleTemplate), span.ToConsoleTemplate()));
//        }
    }
}