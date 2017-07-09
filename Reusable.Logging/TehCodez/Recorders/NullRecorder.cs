namespace Reusable.Loggex.Recorders
{
    public class NullRecorder : IRecorder
    {
        public CaseInsensitiveString Name { get; set; } = nameof(NullRecorder);

        public void Log(LogEntry logEntry) { }
    }
}