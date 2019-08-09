using Reusable.Extensions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public interface ILogEntryBuilder
    {
        string Name { get; }
        
        LogEntry Build();
    }

    // Base interface for the first tier "layer"
    public interface ILogEntryBuilder<T> : ILogEntryBuilder
    {
        ILogEntryBuilder<T> Update(AlterLogEntryCallback alterLogEntry);
    }
    
    public readonly struct LogEntryBuilder<T> : ILogEntryBuilder<T>
    {
        private readonly LogEntry _logEntry;

        public LogEntryBuilder(LogEntry logEntry, string name)
        {
            _logEntry = logEntry;
            Name = name;
        }
        
        public LogEntryBuilder(ILogEntryBuilder other)
        {
            _logEntry = other.Build();
            Name = other.Name;
        }

        public string Name { get; }

        public ILogEntryBuilder<T> Update(AlterLogEntryCallback alterLogEntry) => this.Do(self => alterLogEntry(self._logEntry));

        public LogEntry Build() => _logEntry ?? LogEntry.Empty();
    }
}