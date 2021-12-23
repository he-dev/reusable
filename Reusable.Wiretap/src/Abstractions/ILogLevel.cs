using Reusable.Essentials;
using Reusable.Essentials.Collections;

namespace Reusable.Wiretap.Abstractions
{
    public interface ILogLevel
    {
        SoftString Name { get; }

        [AutoEqualityProperty]
        int Flag { get; }
        
        bool Contains(ILogLevel other);

        bool Contains(int flags);
    }
}