using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    public delegate HtmlElement BuildConsoleTemplateFunc(HtmlElement builder, LogEntry logEntry);

    [PublicAPI]
    public abstract class ConsoleTemplateBuilder<T>
    {
        public abstract T Build(LogEntry logEntry);

        //public virtual BuildConsoleTemplateFunc? Build() => default!;

        //public static ConsoleTemplateBuilder? Null { get; } = default!;
    }

    [PublicAPI]
    public class HtmlConsoleTemplateBuilder : ConsoleTemplateBuilder<HtmlElement>
    {
        public HtmlConsoleTemplateBuilder(bool isParagraph, IConsoleStyle style, IEnumerable<ConsoleTemplateBuilder<HtmlElement>> builders)
        {
            IsParagraph = isParagraph;
            Style = style;
            Builders = builders;
        }

        public IConsoleStyle Style { get; }

        public bool IsParagraph { get; }

        public IEnumerable<ConsoleTemplateBuilder<HtmlElement>> Builders { get; }

        public override HtmlElement Build(LogEntry logEntry)
        {
            var elements = Builders.Select(b => b.Build(logEntry));
            return
                IsParagraph
                    ? HtmlElement.Builder.p(x => x.Append(elements).SetConsoleStyle(Style))
                    : HtmlElement.Builder.span(x => x.Append(elements).SetConsoleStyle(Style));
        }
    }

    internal static class HtmlElementExtensions
    {
        public static HtmlElement SetConsoleStyle(this HtmlElement element, IConsoleStyle style)
        {
            return
                element
                    .backgroundColor(style.BackgroundColor)
                    .color(style.ForegroundColor);
        }
    }
}