using JetBrains.Annotations;

namespace Reusable.OmniLog.SemanticExtensions
{
    [PublicAPI]
    public interface IStateSerializer
    {
        object SerializeObject(object obj);
    }
}