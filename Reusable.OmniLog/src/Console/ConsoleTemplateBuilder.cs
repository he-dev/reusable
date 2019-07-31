using System.Collections.Generic;
using System.Linq;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Console
{
    public abstract class ConsoleTemplateBuilder
    {
        public abstract HtmlElement Build(ILog log);

        public static ConsoleTemplateBuilder Null { get; } = default;
    }
    
    public class CompositeConsoleTemplateBuilder : ConsoleTemplateBuilder
    {
        public bool IsParagraph { get; set; }

        public ConsoleStyle Style { get; set; }

        public IEnumerable<ConsoleTemplateBuilder> Builders { get; set; }

        public override HtmlElement Build(ILog log)
        {
            var elements = Builders.Select(b => b.Build(log));
            return
                IsParagraph
                    ? HtmlElement.Builder.p(x => x.Append(elements).ConsoleStyle(Style))
                    : HtmlElement.Builder.span(x => x.Append(elements).ConsoleStyle(Style));
        }
    }
    
    public static class HtmlElementExtensions
    {
        public static HtmlElement ConsoleStyle(this HtmlElement element, ConsoleStyle style)
        {
            return
                element
                    .backgroundColor(style.BackgroundColor)
                    .color(style.ForegroundColor);
        }
    }
}