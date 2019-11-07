using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    // Makes sure that all types that support a comparer are consistent.
    public interface ICustomizableComparer
    {
        [JsonProperty("Comparer")]
        string ComparerName { get; set; }
    }
    
    [PublicAPI]
    public abstract class Comparer : ScalarExtension<bool>, ICustomizableComparer
    {
        private readonly Func<int, bool> _predicate;

        protected Comparer(ILogger? logger, string name, Func<int, bool> predicate)
            : base(logger, name) => _predicate = predicate;
        
        public IExpression Left { get => ThisInner; set => ThisInner = value; }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; }
        
        public string ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var comparer = context.GetComparerOrDefault(ComparerName);
            var result = comparer.Compare(This(context).Invoke(context).Value, Right.Invoke(context).Value);
            return _predicate(result);
        }
    }
}