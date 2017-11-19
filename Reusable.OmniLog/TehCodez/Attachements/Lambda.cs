using System;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class Lambda : LogAttachement
    {
        private readonly Func<Log, object> _compute;

        public Lambda([NotNull] string name, [NotNull] Func<Log, object> compute) : base(name)
        {
            _compute = compute ?? throw new ArgumentNullException(nameof(compute));
        }

        public override object Compute(Log log) => _compute(log);
    }
}