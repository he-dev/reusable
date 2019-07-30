using System;
using JetBrains.Annotations;

namespace Reusable.MarkupBuilder.Html
{
    // Marker interface for HtmlElement extensions.
    public interface IHtmlElement : IMarkupElement
    {
    }

    public class HtmlElement : MarkupElement, IHtmlElement
    {
        public HtmlElement([NotNull] string name) : base(name, StringComparer.OrdinalIgnoreCase)
        {
        }

        [CanBeNull]
        public static HtmlElement Builder => default;

        public static IMarkupElement Create(string name) => new HtmlElement(name);

        public static implicit operator string(HtmlElement htmlElement) => htmlElement.ToHtml();
    }
}