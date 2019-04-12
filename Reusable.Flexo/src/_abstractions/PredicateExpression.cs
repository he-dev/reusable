using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class PredicateExpression : Expression<bool>
    {
        protected PredicateExpression(ILogger logger, string name) : base(logger, name) { }
    }
}