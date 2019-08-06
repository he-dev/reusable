using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    public delegate HtmlElement BuildConsoleTemplateFunc(HtmlElement builder, LogEntry logEntry);

    public abstract class HtmlConsoleTemplateBuilder
    {
        public static readonly string LogItemTag = nameof(HtmlConsoleTemplateBuilder);

        public abstract HtmlElement Build(LogEntry logEntry);

        public virtual BuildConsoleTemplateFunc Build() => default;

        public static HtmlConsoleTemplateBuilder Null { get; } = default;
    }

    public class CompositeHtmlConsoleTemplateBuilder : HtmlConsoleTemplateBuilder
    {
        public IConsoleStyle Style { get; set; }

        public bool IsParagraph { get; set; }

        public IEnumerable<HtmlConsoleTemplateBuilder> Builders { get; set; }

        public override HtmlElement Build(LogEntry logEntry)
        {
            var elements = Builders.Select(b => b.Build(logEntry));
            var style = Style ?? throw new InvalidOperationException($"You need to set {nameof(Style)} first.");
            return
                IsParagraph
                    ? HtmlElement.Builder.p(x => x.Append(elements).ConsoleStyle(style))
                    : HtmlElement.Builder.span(x => x.Append(elements).ConsoleStyle(style));
        }
    }

    internal static class HtmlElementExtensions
    {
        public static HtmlElement ConsoleStyle(this HtmlElement element, IConsoleStyle style)
        {
            return
                element
                    .backgroundColor(style.BackgroundColor)
                    .color(style.ForegroundColor);
        }
    }
}