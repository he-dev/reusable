using System.Collections.Generic;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogEntry : IEnumerable<LogProperty>
    {
        LogProperty? this[string name] { get; }

        void Add(LogProperty property);

        bool TryGetProperty(string name, out LogProperty property);

        ILogEntry Copy();
    }
}