using System;
using System.Linq;
using Reusable.Collections;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;

namespace Reusable.ConsoleColorizer
{
//    public static class ConsoleTemplateExtensions
//    {
//        public static T ConsoleParagraph<T>(this T template) where T : class, IConsoleTemplate
//        {
//            return template.Element("p");
//        }
//
//        public static T ConsoleText<T>(this T template, string text) where T : class, IConsoleTemplate
//        {
//            return template.Append(text);
//        }
//
////        public static T ConsoleSpan<T>(this T template, string text, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null) where T : class, IConsoleTemplate
////        {
////            var fcolor = foregroundColor.HasValue ? $"color: {foregroundColor.ToString().ToLower()};" : null;
////            var bcolor = backgroundColor.HasValue ? $"background-color: {backgroundColor.ToString().ToLower()};" : null;
////            return template.Element("span", span => span.Append(text).Style(fcolor, bcolor));
////        }
//        
//        public static T ConsoleSpan<T>(this T template, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, params Func<T, object>[] content) where T : class, IConsoleTemplate
//        {
//            return template.Element(
//                name: "span", 
//                local: new
//                {
//                    foregroundColor,
//                    backgroundColor,
//                    content
//                },
//                body: (span, local) =>
//            {
//                var fcolor = local.foregroundColor.HasValue ? $"color: {local.foregroundColor.ToString().ToLower()};" : null;
//                var bcolor = local.backgroundColor.HasValue ? $"background-color: {local.backgroundColor.ToString().ToLower()};" : null;
//
//                span.style(fcolor, bcolor);
//                
//                // Exclude console template because they have already been added.
//                var items = local.content.Select(factory => factory(span)).Where(item => !(item is ConsoleTemplate));
//                foreach (var item in items)
//                {
//                    span.Add(item);
//                }
//            });
//        }
//
//        public static string ToConsoleTemplate<T>(this T template) where T : class, IConsoleTemplate
//        {
//            return template.ToHtml(HtmlFormatting.Empty);
//        }
//    }
}