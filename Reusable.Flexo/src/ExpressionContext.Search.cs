using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;
using Reusable.Quickey;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    public static partial class ExpressionContext
    {
        public static IEnumerable<T> FindItems<T>(this IImmutableContainer context, string key, Func<T, bool> predicate)
        {
            foreach (var current in context.Scopes())
            {
                if (current.TryGetItem(key, out var obj) && obj is T value && predicate(value))
                {
                    yield return value;
                }
            }
        }
        
        /// <summary>
        /// Searches all contexts for items with the specified key and type.
        /// </summary>
        public static IEnumerable<T> FindItems<T>(this IImmutableContainer context, string key)
        {
            foreach (var current in context.Scopes())
            {
                if (current.TryGetItem(key, out var obj) && obj is T value)
                {
                    yield return value;
                }
            }
        }

        public static IEnumerable<T> FindItems<T>(this IImmutableContainer context, Selector<T> key)
        {
            return context.FindItems<T>(key.ToString());
        }
        
        /// <summary>
        /// Searches for the first item with the specified key and type. Throws if now found.
        /// </summary>
        public static T FindItem<T>(this IImmutableContainer context, string key)
        {
            return context.FindItems<T>(key).Take(1).ToList() switch
            {
                {} items when items.Any() => items.Single(),
                _ => throw DynamicException.Create("ItemNotFound", $"Could not find item {key} in any context.")
            };
        }

        public static T FindItem<T>(this IImmutableContainer context, Selector<T> key)
        {
            return context.FindItem<T>(key.ToString());
        }

        public static IEnumerable<TValue> FindItems<TKey, TValue>(this IImmutableContainer context, Selector<IDictionary<TKey, TValue>> mainKey, TKey subKey)
        {
            foreach (var current in context.Scopes())
            {
                if (current.TryGetItem(mainKey, out var dictionary) && dictionary.TryGetValue(subKey, out var value))
                {
                    yield return value;
                }
            }
        }
        
        public static TValue FindItem<TKey, TValue>(this IImmutableContainer context, Selector<IDictionary<TKey, TValue>> mainKey, TKey subKey)
        {
            return context.FindItems(mainKey, subKey).Take(1).ToList() switch
            {
                {} items when items.Any() => items.Single(),
                _ => throw DynamicException.Create("ItemNotFound", $"Could not find item '{mainKey}/{subKey}' in any context.")
            };
        }

        public static IComparer<object> FindComparer(this IImmutableContainer context, string? name = default)
        {
            name ??= "Default";

            foreach (var current in context.Scopes())
            {
                if (current.GetItemOrDefault(Comparers, ImmutableDictionary<SoftString, IComparer<object>>.Empty).TryGetValue(name!, out var comparer))
                {
                    return comparer;
                }
            }

            throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.");
        }

        public static IExpression FindPackage(this IImmutableContainer context, string packageId)
        {
            foreach (var getPackage in context.FindItems(GetPackageFunc))
            {
                if (getPackage(packageId) is {} package)
                {
                    return package;
                }
            }

            throw DynamicException.Create("PackageNotFound", $"Could not find package '{packageId}'.");
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