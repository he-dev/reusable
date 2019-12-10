using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public static partial class ExpressionContext
    {
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
            return context.FindItems<T>(key).FirstOrThrow(("ItemNotFound", $"Could not find item {key} in any context."));
        }

        public static T FindItem<T>(this IImmutableContainer context, Selector<T> key)
        {
            return context.FindItem<T>(key.ToString());
        }

        public static IEnumerable<T> FindItems<T>(this IImmutableContainer context, Selector<IContainer<string, T>> containerKey, string itemKey)
        {
            return
                from scope in context.Scopes()
                let container = scope.GetItemOrDefault(containerKey)
                where container is {}
                from item in container.GetItem(itemKey)
                select item;
        }

        public static T FindItem<T>(this IImmutableContainer context, Selector<IContainer<string, T>> containerKey, string itemKey)
        {
            return context.FindItems(containerKey, itemKey).FirstOrThrow(("ItemNotFound", $"Could not find item '{containerKey}/{itemKey}' in any scope."));
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