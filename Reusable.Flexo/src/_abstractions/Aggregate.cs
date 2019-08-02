using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class Aggregate : CollectionExpressionExtension<double>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected Aggregate(ILogger logger, string name, [NotNull] Func<IEnumerable<double>, double> aggregate)
            : base(logger, name) => _aggregate = aggregate;
        
        public IEnumerable<IExpression> Values { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        protected override Constant<double> InvokeCore()
        {
            var values = Values.Select(e => e.Invoke()).Values<object>().Cast<double>();
            var result = _aggregate(values);
            return (Name, result);
        }
    }
}