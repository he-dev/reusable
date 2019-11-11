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

            var item = context.FindItem<object>(itemKey);

            try
            {
                return names.Skip(1).Aggregate(item, GetMemberValue);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("InvalidPath", $"{Id} could not resolve path '{Path}'. See the inner exception for details.", inner);
            }
        }

        [NotNull]
        private object GetMemberValue(object obj, string memberName)
        {
            obj = obj switch { IConstant c => c.Value ?? throw new ArgumentException($"{c.Id}'s value is null."), _ => obj };

            var member = obj.GetType().GetMember(memberName).SingleOrThrow
            (
                onEmpty: () => DynamicException.Create("MemberNotFound", $"Type '{obj.GetType().ToPrettyString()}' does not have any members with the name '{memberName}'."),
                onMany: () => DynamicException.Create("MultipleMembersFound", $"Type '{obj.GetType().ToPrettyString()}' has more than one member with the name '{memberName}'.")
            );

            return member switch
            {
                PropertyInfo property => property.GetValue(obj),
                FieldInfo field => field.GetValue(obj),
                _ => throw DynamicException.Create("MemberNotFound", $"Could not find member '{obj.GetType().ToPrettyString()}.{memberName}'.")
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