using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Switch : ScalarExtension<object>
    {
        public Switch() : this(default, nameof(Switch)) { }

        protected Switch(ILogger? logger, SoftString name) : base(logger) { }

        public IExpression Value
        {
            get => Arg;
            set => Arg = value;
        }

        public IEnumerable<SwitchCase> Cases { get; set; }

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
        public IExpression When { get; set; }

        [JsonRequired]
        public IExpression Body { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        public bool Matches(IConstant value, IImmutableContainer context)
        {
            return When switch
            {
                IConstant constant => context.GetEqualityComparerOrDefault(ComparerName).Equals(value.Value, constant.Value),
                {} expression => expression.Invoke(context).Value<bool>(),
                _ => true // If not specified then use it as a default case.
            };
        }
    }
}