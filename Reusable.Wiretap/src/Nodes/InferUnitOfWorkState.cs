using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public class InferUnitOfWorkState : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        if (entry[LogProperty.Names.Category()].Value is nameof(TelemetryCategories.UnitOfWork))
        {
            var snapshot = new Dictionary<string, object>();

            if (entry["state"].Value is string state)
            {
                snapshot.Add(nameof(state), state);
            }
            
            if (entry["itemCount"].Value is int itemCount and > 0)
            {
                snapshot.Add(nameof(itemCount), itemCount);
            }

            if (entry["itemIndex"].Value is int itemIndex)
            {
                snapshot.Add(nameof(itemIndex), itemIndex);
            }

            entry.Push<IRegularProperty>(LogProperty.Names.Snapshot(), snapshot);
        }

        Next?.Invoke(entry);
    }
}