using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public delegate void AlterLogEntryCallback(LogEntry logEntry);

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
}