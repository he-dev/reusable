using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.Essentials;

namespace Reusable.ReMember;

public static class SelectorExtensions
{
    /// <summary>
    /// Adds selectors from T to a collection.
    /// </summary>
    public static IImmutableList<Selector> AddFrom<T>(this IImmutableList<Selector> selectors)
    {
        var newSelectors =
            from p in typeof(T).GetProperties()
            select
                typeof(Selector).IsAssignableFrom(p.PropertyType)
                    ? (Selector)p.GetValue(default)
                    : new Selector(typeof(T), p);

        return selectors.AddRange(newSelectors);
    }

    /// <summary>
    /// Filters selectors by tag names where T is an attribute that returns a collection of tags.
    /// </summary>
    public static IEnumerable<Selector> Where<T>(this IEnumerable<Selector> selectors, params string[] names) where T : Attribute, IEnumerable<string>
    {
        if (!names.Any()) throw new ArgumentException("You need to specify at least one tag.");

        return
            from f in selectors
            where f.Tags<T>().IsSupersetOf(names, SoftString.Comparer)
            select f;
    }

    private static IEnumerable<string> Tags<T>(this Selector selector) where T : Attribute, IEnumerable<string>
    {
        var names =
            from m in selector.Metadata.Member.Ancestors()
            from ts in m.GetCustomAttributes<T>()
            from t in ts
            select t;

        return names.Distinct(SoftString.Comparer);
    }

    /// <summary>
    /// Formats multiple selectors.
    /// </summary>
    public static IEnumerable<string> Format(this IEnumerable<Selector> selectors)
    {
        return
            from s in selectors
            select s.ToString();
    }
}