using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        protected Constant<TItem> FindItem<TItem>(IImmutableSession context, Func<string, string> configurePath = default)
        {
            if (context.TryGetValue((configurePath ?? (p => p))(Path), out var item))
            {
                return (Key: Path, (TItem)item, context);
            }
            else
            {
                throw DynamicException.Create("ItemNotFound", $"Could not find an item with the key '{Path}'.");
            }
        }
    }

    public class GetValue : GetItem<object>
    {
        public GetValue([NotNull] ILogger<GetValue> logger) : base(logger, nameof(GetValue)) { }

        protected override Constant<object> InvokeCore(IImmutableSession context)
        {
            return FindItem<object>(context);
        }
    }

    public class GetCollection : GetItem<IEnumerable<object>>
    {
        public GetCollection([NotNull] ILogger<GetCollection> logger) : base(logger, nameof(GetCollection)) { }

        protected override Constant<IEnumerable<object>> InvokeCore(IImmutableSession context)
        {
            return FindItem<IEnumerable<object>>(context);
        }
    }

    public class Ref : GetItem<IExpression>//, IExtension<object>
    {
        public Ref([NotNull] ILogger<Ref> logger) : base(logger, nameof(Ref)) { }

        protected override Constant<IExpression> InvokeCore(IImmutableSession context)
        {
            var expressions = context.Get(Use<IExpressionSession>.Scope, x => x.Expressions, ImmutableDictionary<SoftString, IExpression>.Empty);
            var path = Path.StartsWith("R.", StringComparison.OrdinalIgnoreCase) ? Path : $"R.{Path}";
            if (expressions.TryGetValue(path, out var expression))
            {
                return (Key: Path, expression, context);
            }
            else
            {
                throw DynamicException.Create("RefNotFound", $"Could not find a reference to '{path}'.");
            }
        }        
    }
}