using JetBrains.Annotations;

namespace Reusable.Wiretap.Abstractions
{
    [PublicAPI]
    public interface ISerialize
    {
        object Invoke(object value);
    }
}