using JetBrains.Annotations;

namespace Reusable.OmniLog
{
    [PublicAPI]
    public interface IStateSerializer
    {
        object SerializeObject(object obj);
    }
}