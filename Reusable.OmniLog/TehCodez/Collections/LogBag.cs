using Reusable.Collections;

namespace Reusable.OmniLog.Collections
{
    /// <summary>
    /// This is a special field used for storing data that log-attachements can use for their computations.
    /// </summary>
    public class LogBag : PainlessDictionary<SoftString, object> { }
}