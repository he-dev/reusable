using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class ComparerExpression : ValueExtension<bool>
    {
        private readonly Func<int, bool> _predicate;

        protected ComparerExpression(ILogger logger, string name, [NotNull] Func<int, bool> predicate)
            : base(logger, name) => _predicate = predicate;

        [JsonProperty("Left")]
        public override IExpression This { get; set; }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; }

        protected override Constant<bool> InvokeCore(IExpression @this)
        {
            var result = Comparer<object>.Default.Compare(@this.Invoke().Value, Right.Invoke().Value);
            return (Name, _predicate(result));
        }
    }
}