using System.Collections.Generic;

namespace Reusable.OmniLog
{
    /// <summary>
    /// Use this interface to implement merging of scope logs.
    /// </summary>
    public interface ILogScopeMerge
    {
        SoftString Name { get; }

        KeyValuePair<SoftString, object> Merge(IEnumerable<KeyValuePair<SoftString, object>> items);
    }
}