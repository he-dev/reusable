using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Gets an item from the context. Calls Invoke if it's an expression, otherwise it's wrapped in a Constant.
    /// </summary>
    [PublicAPI]
    public abstract class GetItem<T> : Expression<T>
    {
        protected GetItem(ILogger? logger) : base(logger) { }

        [JsonRequired]
        public string Path { get; set; } = default!;

        // item.Property.Property --> scope[item].Property.Property

        protected object FindItem(IImmutableContainer context)
        {
            var names = Path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries) switch
            {
                {} x when x.Any() => x,
                _ => throw new InvalidOperationException($"{Id}'s {nameof(Path)} must contain at least one name.")
            };

            var itemKey = MapItemKey(names.First());

            try
            {
                // Since scopes can be nested, we need to search for the path in all of them until there's a hit.
                var objects =
                    from item in context.FindItems<object>(itemKey)
                    let obj = names.Skip(1).Aggregate<string, object?>(item, GetMemberValue)
                    where obj is { }
                    select obj;

                return objects.FirstOrDefault() switch
                {
                    {} obj => obj,
                    _ => throw DynamicException.Create("MemberNotFound", $"Could not find member '{Path}'.")
                };
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("InvalidPath", $"{Id} could not resolve path '{Path}'. See the inner exception for details.", inner);
            }
        }

        private object? GetMemberValue(object? obj, string memberName)
        {
            if (obj is null)
            {
                return default;
            }
            else
            {
                obj = obj switch { IConstant {Count: 1} c => c.Single() ?? throw new ArgumentException($"{c.Id}'s value is null."), _ => obj };

                var member = obj.GetType().GetMember(memberName).SingleOrDefault();

                return member switch
                {
                    PropertyInfo property => property.GetValue(obj),
                    FieldInfo field => field.GetValue(obj),
                    _ => default // Since we search for the member in all objects we don't want to throw anything here.
                };
            }
        }

        private static string MapItemKey(string name)
        {
            return name switch
            {
                {} key when key.In(new[] { "arg", "this", "item", "x", "_" }, SoftString.Comparer) => ExpressionContext.Arg.ToString(),
                {} key => key,
                _ => throw new ArgumentNullException(nameof(name))
            };
        }
    }
}