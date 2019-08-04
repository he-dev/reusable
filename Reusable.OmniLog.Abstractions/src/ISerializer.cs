using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions
{
    [PublicAPI]
    public interface ISerializer
    {
        object Serialize(object obj);
    }
}