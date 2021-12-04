using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services;

public interface IConsoleTemplateBuilder<out T>
{
    T Build(ILogEntry logEntry);
}

public interface IHtmlConsoleTemplateBuilder : IConsoleTemplateBuilder<HtmlElement> { }

[PublicAPI]
public class HtmlConsoleTemplateBuilder : List<IHtmlConsoleTemplateBuilder>, IHtmlConsoleTemplateBuilder
{
    public HtmlConsoleTemplateBuilder(bool isParagraph, IConsoleStyle style, IEnumerable<IHtmlConsoleTemplateBuilder> builders) : base(builders)
    {
        IsParagraph = isParagraph;
        Style = style;
    }

    public IConsoleStyle Style { get; }

    public bool IsParagraph { get; }

    public HtmlElement Build(ILogEntry logEntry)
    {
        var elements = this.Select(b => b.Build(logEntry));
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