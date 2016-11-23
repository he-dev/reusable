using System;

namespace Reusable.Clocks
{
    public interface IClock
    {
        DateTime GetNow();
        DateTime GetUtcNow();
    }
}
