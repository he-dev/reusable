using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class Comparer : ScalarExtension<bool>
    {
        private readonly Func<int, bool> _predicate;

        protected Comparer(ILogger? logger, string name, Func<int, bool> predicate)
            : base(logger, name) => _predicate = predicate;
        
        public IExpression Left { get => ThisInner; set => ThisInner = value; }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; }
        
        [JsonProperty("Comparer")]
        public string ComparerName { get; set; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            var comparer = context.GetComparerOrDefault(ComparerName);
            var result = comparer.Compare(Left.Invoke(context).Value, Right.Invoke(context).Value);
            return _predicate(result);
        }
    }
}