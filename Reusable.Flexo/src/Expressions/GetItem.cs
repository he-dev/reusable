using System;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

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

        // key.Property.Property --> session[key].Property.Property
        // this.Property.Property --> @this.Property.Property

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
                var query =
                    from item in context.FindItems<object>(itemKey)
                    let obj = names.Skip(1).Aggregate(item, GetMemberValue)
                    where obj is { }
                    select obj;

                return query.FirstOrDefault() switch
                {
                    {} obj => obj,
                    _ => throw DynamicException.Create("MemberNotFound", $"Could not find member '{Path}'.")
                };

                // foreach (var item in items)
                // {
                //     var obj = names.Skip(1).Aggregate(item, GetMemberValue);
                //     if (obj is {})
                //     {
                //         return obj;
                //     }
                // }
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("InvalidPath", $"{Id} could not resolve path '{Path}'. See the inner exception for details.", inner);
            }
        }

        private object? GetMemberValue(object obj, string memberName)
        {
            if (obj is null)
            {
                return default;
            }

            obj = obj switch { IConstant c => c.Value ?? throw new ArgumentException($"{c.Id}'s value is null."), _ => obj };

            var member = obj.GetType().GetMember(memberName).SingleOrDefault();
            // (
            //     onEmpty: () => DynamicException.Create("MemberNotFound", $"Type '{obj.GetType().ToPrettyString()}' does not have any members with the name '{memberName}'."),
            //     onMany: () => DynamicException.Create("MultipleMembersFound", $"Type '{obj.GetType().ToPrettyString()}' has more than one member with the name '{memberName}'.")
            // );

            return member switch
            {
                PropertyInfo property => property.GetValue(obj),
                FieldInfo field => field.GetValue(obj),
                _ => default // throw DynamicException.Create("MemberNotFound", $"Could not find member '{obj.GetType().ToPrettyString()}.{memberName}'.")
            };
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