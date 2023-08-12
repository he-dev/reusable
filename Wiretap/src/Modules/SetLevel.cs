using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class SetLevel : IModule
{
    public const string Key = "level";

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (entry.TryGetValue(LogEntry.PropertyNames.Trace, out var trace) && trace is string name)
        {
            next(activity, entry.SetItem(Key, MapStatusToLevel(name)));
        }
    }

    private static string MapStatusToLevel(string trace)
    {
        return trace switch
        {
            nameof(ActivityTraces.LogBegin) => "info",
            nameof(ActivityTraces.LogInfo) => "debug",
            nameof(ActivityTraces.LogEnd) => "info",
            nameof(ActivityTraces.LogNoop) => "debug",
            nameof(ActivityTraces.LogStop) => "warning",
            nameof(ActivityTraces.LogError) => "error",
            _ => "warning"
        };
    }
}