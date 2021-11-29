using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Extension<object, object>
    {
        public Switch() : this(default) { }

        protected Switch(ILogger? logger) : base(logger) { }

        public IExpression? Value
        {
            //get => Arg;
            set => Arg = value;
        }
        
        public IEnumerable<SwitchCase>? Cases { get; set; } = default!;

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            var value = GetArg(context).Invoke(context);
            var scope = context.BeginScopeWithArg(value);

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                if (switchCase.Matches(value, scope))
                {
                    return switchCase.Body.Invoke(scope);
                }
            }

            throw DynamicException.Create("SwitchValueOutOfRange", $"'{value}' didn't match any case.");
        }
    }

    public class SwitchCase
    {
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [JsonRequired]
        public IExpression When { get; set; } = default!;

        [JsonRequired]
        public IExpression Body { get; set; } = default!;

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        public bool Matches(IConstant value, IImmutableContainer context)
        {
            return When switch
            {
                IConstant constant => value.Cast<object>().SequenceEqual(constant.Cast<object>(), context.FindItem(ExpressionContext.EqualityComparers, ComparerName ?? Keywords.Default)),
                {} expression => expression.Invoke(context).Cast<bool>().All(b => b),
                _ => true // If not specified then use it as a default case.
            };
        }
    }
}