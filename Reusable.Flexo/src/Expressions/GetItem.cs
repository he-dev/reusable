using System;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Gets an item from the context. Calls Invoke if it's an expression, otherwise it's wrapped in a Constant.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public abstract class GetItem<T> : Expression<T>
    {
        protected GetItem(ILogger? logger, string name) : base(logger, name) { }

        public string Path { get; set; }

        // key.Property.Property --> session[key].Property.Property
        // this.Property.Property --> @this.Property.Property

        protected object FindItem(IImmutableContainer context, Func<string, string> configurePath = default)
        {
            var names = Path.Split('.');
            var itemKey = names.First();
            return
                context.TryFindItem<object>(itemKey, out var item)
                    ? names.Skip(1).Aggregate(item, GetMemberValue)
                    : throw DynamicException.Create("ContextItemNotFound", $"Could not find an item with the key '{itemKey}' from '{Path}'.");
        }

        [NotNull]
        private object GetMemberValue(object obj, string memberName)
        {
            obj = obj switch { IConstant c => c.Value ?? throw new ArgumentException($"{c.Name.ToString()}'s value is null."), _ => obj };

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
    }

    public class Item : GetItem<object>
    {
        public Item() : base(default, nameof(Item))
        {
            Path = ExpressionContext.Item.ToString();
        }

        protected override Constant<object> ComputeConstantGeneric(IImmutableContainer context)
        {
            return (Path, FindItem(context) switch { IConstant c => c.Value, {} x => x, _ => null }, context);
        }
    }

//    public class Ref : GetItem<IExpression>
//    {
//        public Ref([NotNull] ILogger<Ref> logger) : base(logger, nameof(Ref)) { }
//
//        protected override Constant<IExpression> InvokeAsConstant(IImmutableContainer context)
//        {
//            var expressions = context.GetItemOrDefault(ExpressionContext.Packages);
//            var path = Path.StartsWith("R.", StringComparison.OrdinalIgnoreCase) ? Path : $"R.{Path}";
//
//            return
//                expressions.TryGetValue(path, out var expression)
//                    ? (Path, expression)
//                    : throw DynamicException.Create("RefNotFound", $"Could not find a reference to '{path}'.");
//        }
//    }
}