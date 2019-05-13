using System;
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

namespace Reusable.Flexo
{
    /// <summary>
    /// Gets an item from the context. Calls Invoke if it's an expression, otherwise it's wrapped in a Constant.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public abstract class GetItem<T> : Expression<T>
    {
        //private static readonly SoftString ThisKey = ImmutableSessionKey<IExpressionNamespace>.Create(x => x.This);
        protected GetItem(ILogger logger, string name) : base(logger, name) { }

        public string Path { get; set; }

        // key.Property.Property --> session[key].Property.Property
        // this.Property.Property --> @this.Property.Property 

        protected object FindItem(Func<string, string> configurePath = default)
        {
            var names = Path.Split('.');
            var key = names.First();
            if (Scope.Context.TryGetValue(key, out var item))
            {
                return
                    names
                        .Skip(1)
                        .Aggregate(item, (current, name) =>
                            current is IConstant constant
                                ? GetValue(constant.Value?.GetType() ?? throw DynamicException.Create("ValueNull", $"Constant '{constant.Name.ToString()}' value is null."), constant.Value, name)
                                : GetValue(current.GetType(), current, name)
                        );
            }
            else
            {
                throw DynamicException.Create("ContextItemNotFound", $"Could not find an item with the key '{key}' from '{Path}'.");
            }
        }

        object GetValue(Type type, object obj, string memberName)
        {
            var member =
                type
                    .GetMember(memberName)
                    .SingleOrThrow
                    (
                        onEmpty: () => DynamicException.Create("MemberNotFound", $"Type '{type.ToPrettyString()}' does not have any members with the name '{memberName}'."),
                        onMultiple: () => DynamicException.Create("MultipleMembersFound", $"Type '{type.ToPrettyString()}' has more than one member with the name '{memberName}'.")
                    );

            switch (member)
            {
                case PropertyInfo property: return property.GetValue(obj);
                case FieldInfo field: return field.GetValue(obj);
                default: return null; // this will never occur
            }
        }
    }

    public class Item : GetItem<object>
    {
        public Item([NotNull] ILogger<Item> logger) : base(logger, nameof(Item))
        {
            Path = Reusable.Data.ImmutableSessionKey<IExpressionNamespace>.Create(x => x.Item);
        }

        protected override Constant<object> InvokeCore()
        {
            return (Path, (FindItem() is var item && item is IConstant c ? c.Value : item));
        }
    }

    public class Ref : GetItem<IExpression>
    {
        public Ref([NotNull] ILogger<Ref> logger) : base(logger, nameof(Ref)) { }

        protected override Constant<IExpression> InvokeCore()
        {
            var expressions = Scope.Context.Get(Namespace, x => x.References);
            var path = Path.StartsWith("R.", StringComparison.OrdinalIgnoreCase) ? Path : $"R.{Path}";
            if (expressions.TryGetValue(path, out var expression))
            {
                return (Path, expression);
            }
            else
            {
                throw DynamicException.Create("RefNotFound", $"Could not find a reference to '{path}'.");
            }
        }
    }
}