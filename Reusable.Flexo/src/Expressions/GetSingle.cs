using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class GetSingle : GetItem<object>
    {
        public GetSingle([NotNull] ILogger<GetSingle> logger) : base(logger, nameof(GetSingle)) { }

        protected override Constant<object> InvokeCore(IImmutableContainer context)
        {
            return (Path, FindItem());
        }
    }
}