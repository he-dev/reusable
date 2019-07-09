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
        public static IImmutableContainer WithComparer(this IImmutableContainer context, string name, IEqualityComparer<object> comparer)
        {
            var comparers =
                context
                    .GetItemOrDefault(ExpressionContext.Comparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                    .SetItem(name, comparer);

            return context.SetItem(ExpressionContext.Comparers, comparers);
        }

        public static IImmutableContainer WithDefaultComparer(this IImmutableContainer context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparer<object>.Default);
        }

        public static IImmutableContainer WithSoftStringComparer(this IImmutableContainer context)
        {
            return context.WithComparer(GetComparerNameFromCaller(), EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            ));
        }

        public static IImmutableContainer WithRegexComparer(this IImmutableContainer context)
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

        public static IImmutableContainer WithReferences(this IImmutableContainer context, IEnumerable<IExpression> expressions)
        {
            var registrations =
                context
                    .GetItemOrDefault(ExpressionContext.References, ImmutableDictionary<SoftString, IExpression>.Empty)
                    .SetItems(expressions.Select(e => new KeyValuePair<SoftString, IExpression>($"R.{e.Name.ToString()}", e)));
            return context.SetItem(ExpressionContext.References, registrations);
        }

        public static TResult Find<TResult>(this ExpressionScope scope, Selector<TResult> key)
        {
            foreach (var current in scope.Enumerate())
            {
                if (current.Context.TryGetValue(key.ToString(), out var obj) && obj is TResult value)
                {
                    return value;
                }
            }

            return default;
        }

        public static IEqualityComparer<object> GetComparerOrDefault(this ExpressionScope scope, [CanBeNull] string name)
        {
            return
                scope.Find(ExpressionContext.Comparers).TryGetValue(name ?? "Default", out var comparer)
                    ? comparer
                    : throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.");
        }

        public static TreeNode<ExpressionDebugView> DebugView(this IImmutableContainer context)
        {
            return context.GetItemOrDefault(ExpressionContext.DebugView);
        }

        public static IImmutableContainer DebugView(this IImmutableContainer context, TreeNode<ExpressionDebugView> debugView)
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
                    ? context.GetItemOrDefault(ExpressionContext.This)
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

                    var index = context.GetItemOrDefault(ExpressionContext.This);
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