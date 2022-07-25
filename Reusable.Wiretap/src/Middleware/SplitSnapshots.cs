using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

public class SplitSnapshots : LoggerMiddleware
{
    public override void Invoke(ILogEntry entry)
    {
        if (entry.TryPeek(LogProperty.Names.Snapshots(), out var split))
        {
            // We won't need it anymore.
            entry.Push(new LogProperty.Obsolete(split.Name));
            
            // Optimized for performance.
            using var e = split.Value.GetProperties().GetEnumerator();

            // Use the original entry first...
            if (e.MoveNext())
            {
                entry.Identifier(e.Current.Name);
                entry.Snapshot(e.Current.Value);
            }

            // ...and copy it only when  more entries are required.
            while (e.MoveNext())
            {
                var copy = new LogEntry(entry);
                copy.Identifier(e.Current.Name);
                copy.Snapshot(e.Current.Value);
            }

            Next?.Invoke(entry);
        }
        else
        {
            Next?.Invoke(entry);
        }
    }
}