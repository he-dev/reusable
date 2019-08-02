using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class Comparer : ValueExtension<bool>
    {
        private readonly Func<int, bool> _predicate;

        protected Comparer(ILogger logger, string name, [NotNull] Func<int, bool> predicate)
            : base(logger, name) => _predicate = predicate;
        
        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            var result = Comparer<object>.Default.Compare(Left.Invoke().Value, Right.Invoke().Value);
            return (Name, _predicate(result));
        }
    }
}