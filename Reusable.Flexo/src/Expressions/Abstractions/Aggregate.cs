using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    [PublicAPI]
    public abstract class Aggregate : CollectionExtension<double>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected Aggregate(ILogger? logger, string name, Func<IEnumerable<double>, double> aggregate)
            : base(logger) => _aggregate = aggregate;

        public IEnumerable<IExpression> Values
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        protected override double ComputeValue(IImmutableContainer context)
        {
            var values =
                This(context)
                    .Enabled()
                    .Select(e => e.Invoke(context))
                    .Values<double>();

            return _aggregate(values);
        }
    }
}