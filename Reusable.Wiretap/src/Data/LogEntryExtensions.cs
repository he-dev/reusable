namespace Reusable.Wiretap.Data;

public static class LogEntryExtensions
{
    public static T GetValueOrDefault<T>(this LogEntry entry, string key, T defaultValue)
    {
        return
            entry.TryGetValue(key, out var value) && value is T result
                ? result
                : defaultValue;
    }
}