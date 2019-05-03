using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Xml;
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

        protected object FindItem(Func<string, string> configurePath = default)
        {
            var names = Path.Split('.');
            var key = names.First();
            var obj =
                key == "this"
                    ? Scope.Context.Get(Namespace, x => x.This)
                    : Scope.Context.TryGetValue(key, out var item)
                        ? item
                        : throw DynamicException.Create("ItemNotFound", $"Could not find an item with the key '{Path}'.");

            obj = names.Skip(1).Aggregate(obj, (current, name) => current.GetType().GetProperty(name).GetValue(current));

            return obj;
        }
    }

    public class GetSingle : GetItem<object>
    {
        public GetSingle([NotNull] ILogger<GetSingle> logger) : base(logger, nameof(GetSingle)) { }

        protected override Constant<object> InvokeCore()
        {
            return (Path, FindItem());
        }
    }

    public class GetMany : GetItem<IEnumerable<IExpression>>
    {
        public GetMany([NotNull] ILogger<GetMany> logger) : base(logger, nameof(GetMany)) { }

        protected override Constant<IEnumerable<IExpression>> InvokeCore()
        {
            return (Path, ((IEnumerable<object>)FindItem()).Select((x, i) => Constant.Create($"Item[{i}]", x)).ToList());
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