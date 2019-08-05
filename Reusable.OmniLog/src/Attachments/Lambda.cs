using System;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Attachments
{
    public class Lambda : LogAttachment
    {
        private readonly Func<Log, object> _compute;

        public Lambda([NotNull] string name, [NotNull] Func<Log, object> compute) : base(name)
        {
            _compute = compute ?? throw new ArgumentNullException(nameof(compute));
        }

        public override object Compute(Log log) => _compute(log);
    }
}