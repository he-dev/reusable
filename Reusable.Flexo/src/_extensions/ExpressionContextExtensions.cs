using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
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
        public static IImmutableSession WithComparer(this IImmutableSession context, string name, IEqualityComparer<object> comparer)
        {
            var comparers =
                context
                    .GetItemOrDefault(From<IExpressionMeta>.Select(m => m.Comparers), ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                    .SetItem(name, comparer);

            return context.SetItem(From<IExpressionMeta>.Select(m => m.Comparers), comparers);
        }

        public static IImmutableSession WithDefaultComparer(this IImmutableSession context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparer<object>.Default);
        }

        public static IImmutableSession WithSoftStringComparer(this IImmutableSession context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            ));
        }

        public static IImmutableSession WithRegexComparer(this IImmutableSession context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => Regex.IsMatch((string)right, (string)left, RegexOptions.None),
                getHashCode: (obj) => 0
            ));
        }

        public static string GetComparerNameFromCaller([CallerMemberName] string comparerName = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Regex.Match(comparerName, "^With(?<comparerName>[a-z0-9_]+)Comparer", RegexOptions.IgnoreCase).Groups["comparerName"].Value;
        }

        public static IImmutableSession WithReferences(this IImmutableSession context, IEnumerable<IExpression> expressions)
        {
            var registrations =
                context
                    .GetItemOrDefault(From<IExpressionMeta>.Select(m => m.References), ImmutableDictionary<SoftString, IExpression>.Empty)
                    .SetItems(expressions.Select(e => new KeyValuePair<SoftString, IExpression>($"R.{e.Name.ToString()}", e)));
            return context.SetItem(From<IExpressionMeta>.Select(m => m.References), registrations);
        }

        public static TResult Find<TResult>(this ExpressionScope scope, Selector<TResult> key)
        {
            foreach (var current in scope.Enumerate())
            {
                if (current.Context.TryGetValue(key, out var obj) && obj is TResult value)
                {
                    return value;
                }
            }

            return default;
        }

        public static IEqualityComparer<object> GetComparerOrDefault(this ExpressionScope scope, [CanBeNull] string name)
        {
            return
                scope.Find(From<IExpressionMeta>.Select(m => m.Comparers)).TryGetValue(name ?? "Default", out var comparer)
                    ? comparer
                    : throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.");
        }

        public static TreeNode<ExpressionDebugView> DebugView(this IImmutableSession context)
        {
            return context.GetItemOrDefault(From<IExpressionMeta>.Select(m => m.DebugView));
        }

        public static IImmutableSession DebugView(this IImmutableSession context, TreeNode<ExpressionDebugView> debugView)
        {
            return context.SetItem(From<IExpressionMeta>.Select(m => m.DebugView), debugView);
        }

        public static (object Object, PropertyInfo Property, object Value) FindItem(this IImmutableSession context, string path)
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
                    ? context.GetItemOrDefault(From<IExpressionMeta>.Select(m => m.This))
                    : context.TryGetValue(first.Name, out var item)
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

                    var index = context.GetItemOrDefault(From<IExpressionMeta>.Select(m => m.This));
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