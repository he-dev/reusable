using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public class InferUnitOfWorkResult : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        if (entry[LogProperty.Names.Category()].Value is nameof(TelemetryCategories.UnitOfWork) && entry[nameof(TelemetryCategories.Auto)].Value is true)
        {
            if (entry[LogProperty.Names.LoggerScope()].Value is not LoggerScope loggerScope)
            {
                throw new InvalidOperationException("You must create a logger-scope with BeginScope before you can use UnitOfWork's Auto option.");
            }

            if (loggerScope.Items.TryGetValue(nameof(Exception), out var value) && value is Exception exception)
            {
                entry.Push<IRegularProperty>(LogProperty.Names.Snapshot(), new { state = nameof(TelemetryCategories.Faulted) });
                entry.Push<IRegularProperty>(LogProperty.Names.Exception(), exception);
            }
            else
            {
                entry.Push<IRegularProperty>(LogProperty.Names.Snapshot(), new { state = nameof(TelemetryCategories.Completed) });
            }
        }

        Next?.Invoke(entry);
    }
}