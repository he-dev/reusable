namespace Reusable.Logging.Loggex.Recorders
{
    public class NullRecorder : IRecorder
    {
        public CaseInsensitiveString Name { get; set; } = "Null";

        public void Log(LogEntry logEntry) { }
    }
}