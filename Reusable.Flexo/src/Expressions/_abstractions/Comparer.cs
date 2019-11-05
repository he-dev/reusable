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

        protected Comparer(ILogger logger, string name, [NotNull] Func<int, bool> predicate)
            : base(logger, name) => _predicate = predicate;
        
        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var result = Comparer<object>.Default.Compare(Left.Invoke(TODO).Value, Right.Invoke(TODO).Value);
            return (Name, _predicate(result));
        }
    }
}