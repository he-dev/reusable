using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    public static partial class ExpressionContext
    {
        public static T FindItem<T>(this IImmutableContainer scope, Selector<T> key)
        {
            var keyToFind = key.ToString();
            foreach (var current in scope.Scopes())
            {
                if (current.TryGetItem(keyToFind, out var obj) && obj is T value)
                {
                    return value;
                }
            }

            return default;
        }

        public static IEnumerable<T> FindItems<T>(this IImmutableContainer context, Selector<T> key)
        {
            var keyToFind = key.ToString();
            foreach (var current in context.Scopes())
            {
                if (current.TryGetItem(keyToFind, out var obj) && obj is T value)
                {
                    yield return value;
                }
            }
        }

        public static bool TryFindItem<T>(this IImmutableContainer context, string keyToFind, out T item)
        {
            foreach (var current in context.Scopes())
            {
                if (current.TryGetItem(keyToFind, out var obj) && obj is T value)
                {
                    item = value;
                    return true;
                }
            }

            item = default;
            return false;
        }

        public static IEqualityComparer<object> FindEqualityComparer(this IImmutableContainer context, string? name = default)
        {
            foreach (var current in context.Scopes())
            {
                if (current.GetItemOrDefault(EqualityComparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty).TryGetValue(name ?? "Default", out var comparer))
                {
                    return comparer;
                }
            }

            throw DynamicException.Create("EqualityComparerNotFound", $"There is no equality-comparer with the name '{name}'.");
        }

        public static IComparer<object> FindComparer(this IImmutableContainer context, string? name)
        {
            foreach (var current in context.Scopes())
            {
                if (current.GetItemOrDefault(Comparers, ImmutableDictionary<SoftString, IComparer<object>>.Empty).TryGetValue(name ?? "Default", out var comparer))
                {
                    return comparer;
                }
            }

            throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.");
        }
        
        public static (object Object, PropertyInfo Property, object Value) FindMember(this IImmutableContainer context, string path)
        {
            // Supported paths: sth.Property[index].Property
            var names =
                Regex
                    .Matches(path, @"((?<name>[a-z0-9_]+)(\[(?<item>[a-z0-9_]+)\])?)", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(m => (Name: m.Group("name"), Item: m.Group("item")))
                    .ToList();

            if (names.Count < 2)
            {
                throw DynamicException.Create("PathTooShort", "Path needs to contain at least two names separated by '.'.");
            }

            // Get the initial object from the context.
            var first = names.First();
            var obj =
                first.Name == "this"
                    ? context.GetItemOrDefault(ExpressionContext.Arg)
                    : context.TryGetItem(first.Name, out var item)
                        ? item
                        : throw DynamicException.Create("InitialObjectNotFound", $"Could not find an item with the key '{path}'.");

            // Follow the path and try to get the corresponding property for each name.
            return names.Skip(1).Aggregate((Object: obj, Property: default(PropertyInfo), Value: default(object)), (current, next) =>
            {
                if (current.Object.GetType().GetProperty(next.Name) is var property && !(property is null))
                {
                    var value = property.GetValue(current.Object);

                    if (string.IsNullOrEmpty(next.Item))
                    {
                        return (current.Object, property, value);
                    }

                    var index = context.GetItemOrDefault(ExpressionContext.Arg);
                    var element = ((IEnumerable<object>)value).Single(x => x.Equals(index));
                    return (element, default, default);
                }
                else
                {
                    throw DynamicException.Create
                    (
                        $"PropertyNotFound",
                        $"Type '{current.GetType().ToPrettyString()}' does not have such property as '{next.Name}'."
                    );
                }
            });
        }
    }
}