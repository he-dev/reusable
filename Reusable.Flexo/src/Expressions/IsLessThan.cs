using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    //[Alias("<")]
    public class IsLessThan : Comparer
    {
        public IsLessThan() : base(default, nameof(IsLessThan), x => x < 0) { }
    }

    //[Alias("<=")]
    public class IsLessThanOrEqual : Comparer
    {
        public IsLessThanOrEqual() : base(default, nameof(IsLessThanOrEqual), x => x <= 0) { }
    }
}