using System.Linq.Custom;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public class SplitSnapshots : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        if (entry.TryPeek(LogProperty.Names.Snapshots(), out var split))
        {
            // We won't need it anymore.
            entry.Push(new LogProperty.Obsolete(split.Name));
            
            // Optimized for performance.
            using var e = split.GetProperties().GetEnumerator();

            // Use the original entry first...
            if (e.MoveNext())
            {
                entry.Push<IRegularProperty>(LogProperty.Names.Tag(), e.Current.Name);
                entry.Push<ITransientProperty>(LogProperty.Names.Snapshot(), e.Current.Value);
            }

            // ...and copy it only when  more entries are required.
            while (e.MoveNext())
            {
                var copy = new LogEntry(entry);
                copy.Push<IRegularProperty>(LogProperty.Names.Tag(), e.Current.Name);
                copy.Push<ITransientProperty>(LogProperty.Names.Snapshot(), e.Current.Value);
            }

            Next?.Invoke(entry);
        }
        else
        {
            Next?.Invoke(entry);
        }
    }
}