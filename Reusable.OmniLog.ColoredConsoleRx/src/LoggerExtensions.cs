using System;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;

namespace Reusable.OmniLog
{
    public static class LoggerExtensions
    {
        public static ILogger Write(this ILogger logger, Func<HtmlElement, HtmlElement> paragraphAction)
        {
            var html = paragraphAction(HtmlElement.Builder.span()).ToHtml(HtmlFormatting.Empty);

            return logger.Log(LogLevel.Information, log => log.With(nameof(Write), html));
        }
        
        public static ILogger WriteLine(this ILogger logger, Func<HtmlElement, HtmlElement> paragraphAction)
        {
            var html = paragraphAction(HtmlElement.Builder.p()).ToHtml(HtmlFormatting.Empty);

            return logger.Log(LogLevel.Information, log => log.With(nameof(Write), html));
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