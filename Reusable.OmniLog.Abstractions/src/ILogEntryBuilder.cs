namespace Reusable.OmniLog.Abstractions
{
    public delegate void ProcessLogEntryDelegate(ILogEntry logEntry);

    public interface ILogEntryBuilder
    {
        string Name { get; }

        ILogEntry Build();
    }

    // Base interface for the first tier "layer"
    public interface ILogEntryBuilder<T> : ILogEntryBuilder
    {
        ILogEntryBuilder<T> Update(ProcessLogEntryDelegate processLogEntry);
    }
}