using System;
using Reusable.Collections;

namespace Reusable.OmniLog.Abstractions
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