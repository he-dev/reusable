namespace Reusable.Logging.Loggex
{
    public interface IRecorder
    {
        CaseInsensitiveString Name { get; set; }

        void Log(LogEntry logEntry);
    }
}
