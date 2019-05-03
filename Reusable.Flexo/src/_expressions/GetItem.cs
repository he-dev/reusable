using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
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
        protected GetItem(ILogger logger, string name) : base(logger, name) { }

        [JsonRequired]
        public string Path { get; set; }

        // key.Property.Property --> session[key].Property.Property
        // this.Property.Property --> @this.Property.Property 

        protected Constant<TItem> FindItem<TItem>(Func<string, string> configurePath = default)
        {
            var names = Path.Split('.');

            var obj =
                names.First() == "this"
                    ? Scope.Context["this"] //Expression.This.Pop() // context.PopThis()
                    : Scope.Context.TryGetValue(names.First(), out var item)
                        ? item
                        : throw DynamicException.Create("ItemNotFound", $"Could not find an item with the key '{Path}'.");

            obj = names.Skip(1).Aggregate(obj, (current, name) => current.GetType().GetProperty(name).GetValue(current));

            return (Key: Path, (TItem)obj);
        }
    }

    public class GetValue : GetItem<object>
    {
        public GetValue([NotNull] ILogger<GetValue> logger) : base(logger, nameof(GetValue)) { }

        protected override Constant<object> InvokeCore()
        {
            return FindItem<object>();
        }
    }

    public class GetCollection : GetItem<IEnumerable<object>>
    {
        public GetCollection([NotNull] ILogger<GetCollection> logger) : base(logger, nameof(GetCollection)) { }

        protected override Constant<IEnumerable<object>> InvokeCore()
        {
            return FindItem<IEnumerable<object>>();
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
                return (Key: Path, expression);
            }
            else
            {
                throw DynamicException.Create("RefNotFound", $"Could not find a reference to '{path}'.");
            }
        }
    }
}