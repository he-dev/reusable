using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    [PublicAPI]
    public abstract class Aggregate : Extension<double, double>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected Aggregate(ILogger? logger, Func<IEnumerable<double>, double> aggregate) : base(logger) => _aggregate = aggregate;

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        protected override double ComputeSingle(IImmutableContainer context)
        {
            var values = GetArg(context).Cast<double>();
            return _aggregate(values);
        }
    }
}