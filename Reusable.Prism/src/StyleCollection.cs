using System.Collections;
using System.Collections.Generic;
using Reusable.Marbles;

namespace Reusable.Prism;

public class StyleCollection : IEnumerable<KeyValuePair<string, Style>>
{
    private IDictionary<string, Style> Styles { get; } = new Dictionary<string, Style>(SoftString.Comparer);

    public static StyleCollection Empty() => new();

    public Style this[string name] => Styles.TryGetValue(name, out var style) ? style : throw DynamicException.Create($"Console style '{name}' not found.");

    IEnumerator<KeyValuePair<string, Style>> IEnumerable<KeyValuePair<string, Style>>.GetEnumerator() => Styles.GetEnumerator();

    public IEnumerator GetEnumerator() => ((IEnumerable)Styles).GetEnumerator();

    public void Add(string name, Style style) => Styles.Add(name, style);
}

public static class StyleCollectionExtensions
{
    public static StyleCollection Add<T>(this StyleCollection css) where T : Style, new()
    {
        return css.Also(x => x.Add(typeof(T).Name, new T()));
    }
}