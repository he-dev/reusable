using System;

namespace Reusable.Clocks
{
    public interface IClock
    {
        DateTime Now();
        DateTime UtcNow();
    }
}
