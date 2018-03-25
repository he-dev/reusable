using JetBrains.Annotations;

namespace Reusable.OmniLog
{
    [PublicAPI]
    public interface ISerializer
    {
        object Serialize(object obj);
    }
}