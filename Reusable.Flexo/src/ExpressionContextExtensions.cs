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
    [PublicAPI]
    public static class ExpressionContextExtensions
    {
        public static IImmutableContainer WithEqualityComparer(this IImmutableContainer context, string name, IEqualityComparer<object> comparer)
        {
            return context.UpdateItem(ExpressionContext.EqualityComparers, x => x.SetItem(name, comparer));

            var comparers =
                context
                    .GetItemOrDefault(ExpressionContext.EqualityComparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                    .SetItem(name, comparer);

            return context.SetItem(ExpressionContext.EqualityComparers, comparers);
        }

        public static IImmutableContainer WithDefaultEqualityComparer(this IImmutableContainer context)
        {
            return context.WithEqualityComparer(nameof(ExpressionContext.Default), EqualityComparer<object>.Default);
        }

        public static IImmutableContainer WithDefaultComparer(this IImmutableContainer context)
        {
            return context.UpdateItem(ExpressionContext.Comparers, x => x.SetItem(nameof(ExpressionContext.Default), Comparer<object>.Default));
        }

        public static IImmutableContainer WithSoftStringComparer(this IImmutableContainer context)
        {
            return context.WithEqualityComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            ));
        }

//        public static IImmutableContainer WithRegexComparer(this IImmutableContainer context)
//        {
//            return context.WithEqualityComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
//            (
//                equals: (left, right) => Regex.IsMatch((string)right, (string)left, RegexOptions.None),
//                getHashCode: (obj) => 0
//            ));
//        }

        public static string GetComparerNameFromCaller([CallerMemberName] string comparerName = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Regex.Match(comparerName, "^With(?<comparerName>[a-z0-9_]+)Comparer", RegexOptions.IgnoreCase).Groups["comparerName"].Value;
        }

        public static IImmutableContainer WithPackages(this IImmutableContainer context, IEnumerable<IExpression> expressions)
        {
            var packages =
                expressions
                    .Aggregate(
                        context.GetItemOrDefault(ExpressionContext.Packages, ImmutableDictionary<SoftString, IExpression>.Empty),
                        (current, next) => next switch
                        {
                            Package p => current.SetItem(p.Id, p),
                            {} x => throw DynamicException.Create("InvalidExpression", $"{x.Id.ToString()} is not a package."),
                            _ => throw DynamicException.Create("PackageNull", "Package must not be null.")
                        });

            return context.SetItem(ExpressionContext.Packages, packages);
        }

        public static TResult FindItem<TResult>(this IImmutableContainer scope, Selector<TResult> key)
        {
            var keyToFind = key.ToString();
            foreach (var current in scope.Enumerate())
            {
                if (current.TryGetItem(keyToFind, out var obj) && obj is TResult value)
                {
                    return value;
                }
            }

            return default;
        }

        public static IEnumerable<TResult> FindItems<TResult>(this IImmutableContainer scope, Selector<TResult> key)
        {
            var keyToFind = key.ToString();
            foreach (var current in scope.Enumerate())
            {
                if (current.TryGetItem(keyToFind, out var obj) && obj is TResult value)
                {
                    yield return value;
                }
            }
        }

        public static bool TryFindItem<TResult>(this IImmutableContainer scope, string keyToFind, out TResult item)
        {
            foreach (var current in scope.Enumerate())
            {
                if (current.TryGetItem(keyToFind, out var obj) && obj is TResult value)
                {
                    item = value;
                    return true;
                }
            }

            item = default;
            return false;
        }

        public static IEqualityComparer<object> GetEqualityComparerOrDefault(this IImmutableContainer scope, string? name = default)
        {
            return scope.FindItem(ExpressionContext.EqualityComparers).TryGetValue(name ?? "Default", out var comparer) switch
            {
                true => comparer,
                false => throw DynamicException.Create("EqualityComparerNotFound", $"There is no equality-comparer with the name '{name}'.")
            };
        }

        public static IComparer<object> GetComparerOrDefault(this IImmutableContainer scope, string? name)
        {
            return scope.FindItem(ExpressionContext.Comparers).TryGetValue(name ?? "Default", out var comparer) switch
            {
                true => comparer,
                false => throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.")
            };
        }

        public static Node<ExpressionDebugView> DebugView(this IImmutableContainer context)
        {
            return context.GetItemOrDefault(ExpressionContext.DebugView);
        }

        public static IImmutableContainer DebugView(this IImmutableContainer context, Node<ExpressionDebugView> debugView)
        {
            return context.SetItem(ExpressionContext.DebugView, debugView);
        }

        public static (object Object, PropertyInfo Property, object Value) FindItem(this IImmutableContainer context, string path)
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
                    ? context.GetItemOrDefault(ExpressionContext.ThisOuter)
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

                    var index = context.GetItemOrDefault(ExpressionContext.ThisOuter);
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