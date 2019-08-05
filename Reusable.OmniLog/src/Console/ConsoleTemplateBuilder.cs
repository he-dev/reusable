using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Console
{
    public delegate HtmlElement BuildConsoleTemplateFunc(HtmlElement builder, Log log);
    
    public abstract class ConsoleTemplateBuilder
    {
        public static readonly string LogItemTag = nameof(ConsoleTemplateBuilder);
        
        public abstract HtmlElement Build(Log log);

        public virtual BuildConsoleTemplateFunc Build() => default;

        public static ConsoleTemplateBuilder Null { get; } = default;
    }
    
    public class CompositeConsoleTemplateBuilder : ConsoleTemplateBuilder
    {
        public bool IsParagraph { get; set; }

        public IEnumerable<ConsoleTemplateBuilder> Builders { get; set; }

        public override HtmlElement Build(Log log)
        {
            var elements = Builders.Select(b => b.Build(log));
            var style = log.ConsoleStyle() ?? throw new InvalidOperationException($"You need to set {nameof(ConsoleStyle)} first.");
            return
                IsParagraph
                    ? HtmlElement.Builder.p(x => x.Append(elements).ConsoleStyle(style))
                    : HtmlElement.Builder.span(x => x.Append(elements).ConsoleStyle(style));
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