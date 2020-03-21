using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public readonly struct LogEntryBuilder<T> : ILogEntryBuilder<T>
    {
        private readonly ILogEntry _logEntry;

        public LogEntryBuilder(ILogEntry logEntry, string name)
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

        public ILogEntryBuilder<T> Update(ProcessLogEntryDelegate processLogEntry) => this.Pipe(self => processLogEntry(self._logEntry));

        public ILogEntry Build() => _logEntry ?? LogEntry.Empty();
    }
}