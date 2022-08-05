using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Marbles;

namespace Reusable.Fluorite;

public interface IHtmlElement : ICollection<object>, IFormattable
{
    string Name { get; }

    IDictionary<string, string> Attributes { get; }

    IHtmlElement? Parent { get; }

    int Depth { get; }
}

[PublicAPI]
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class HtmlElement : Collection<object>, IHtmlElement
{
    public HtmlElement(string name) : this(name, Enumerable.Empty<object>().ToList())
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Attributes = new Dictionary<string, string>(SoftString.Comparer);
    }

    public HtmlElement(string name, IList<object> content) : base(content)
    {
        Name = name;
        Attributes = new Dictionary<string, string>(SoftString.Comparer);
    }

    private string DebuggerDisplay => $"<{Name} attribute-count=\"{Attributes?.Count ?? 0}\" children-count=\"{Count}\">";

    //public static IHtmlElement Builder(string name) => new HtmlElement(name);
    
    #region IHtmlElement

    public string Name { get; }

    public IDictionary<string, string> Attributes { get; }

    public IHtmlElement? Parent { get; private set; }

    public int Depth => Parent?.Depth + 1 ?? 0;

    #endregion
    
    public static IHtmlElement Create(string name) => new HtmlElement(name);
    
    #region Collection

    public bool IsReadOnly => false;

    protected override void InsertItem(int index, object item)
    {
        SetParent(item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, object item)
    {
        SetParent(item);
        base.SetItem(index, item);
    }

    #endregion

    // Sets this element as the parent of the item being added if it's a markup-element.
    private void SetParent(object item)
    {
        if (item is HtmlElement markupElement)
        {
            markupElement.Parent = this;
        }
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return
            formatProvider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter formatter
                ? formatter.Format(format, this, formatProvider)
                : base.ToString() ?? string.Empty;
    }
    
    public static implicit operator string(HtmlElement htmlElement) => htmlElement.ToHtml();

}