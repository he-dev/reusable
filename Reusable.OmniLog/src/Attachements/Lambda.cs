using System;
using JetBrains.Annotations;

namespace Reusable.OmniLog.Attachements
{
    public class Lambda : LogAttachement
    {
        private readonly Func<ILog, object> _compute;

        public Lambda([NotNull] string name, [NotNull] Func<ILog, object> compute) : base(name)
        {
            _compute = compute ?? throw new ArgumentNullException(nameof(compute));
        }

        public override object Compute(ILog log) => _compute(log);
    }
}