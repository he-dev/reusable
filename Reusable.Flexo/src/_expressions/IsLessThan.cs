using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("<")]
    public class IsLessThan : Comparer
    {
        public IsLessThan(ILogger<IsLessThan> logger) : base(logger, nameof(IsLessThan), x => x < 0) { }
    }

    [Alias("<=")]
    public class IsLessThanOrEqual : Comparer
    {
        public IsLessThanOrEqual(ILogger<IsLessThanOrEqual> logger) : base(logger, nameof(IsLessThanOrEqual), x => x <= 0) { }
    }
}