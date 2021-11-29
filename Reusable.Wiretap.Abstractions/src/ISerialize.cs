using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions
{
    [PublicAPI]
    public interface ISerialize
    {
        object Invoke(object obj);
    }
}