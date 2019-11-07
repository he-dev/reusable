using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

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
                    ? names.Skip(1).Aggregate(item, GetValue)
                    : throw DynamicException.Create("ContextItemNotFound", $"Could not find an item with the key '{itemKey}' from '{Path}'.");
        }

        [NotNull]
        private object GetValue(object obj, string memberName)
        {
            var type = default(Type);
            (type, obj) =
                obj is IConstant constant
                    ? (constant.Value?.GetType() ?? throw DynamicException.Create("ValueNull", $"Constant '{constant.Name.ToString()}' value is null."), constant.Value)
                    : (obj.GetType(), obj);

            var member =
                type
                    .GetMember(memberName)
                    .SingleOrThrow
                    (
                        onEmpty: () => DynamicException.Create("MemberNotFound", $"Type '{type.ToPrettyString()}' does not have any members with the name '{memberName}'."),
                        onMany: () => DynamicException.Create("MultipleMembersFound", $"Type '{type.ToPrettyString()}' has more than one member with the name '{memberName}'.")
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