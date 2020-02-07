using System.Collections.Generic;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogEntry : IEnumerable<LogProperty>
    {
        LogProperty? this[SoftString name] { get; }

        void Add(LogProperty property);

        bool TryGetProperty(SoftString name, out LogProperty property);

        ILogEntry Copy();
    }
}