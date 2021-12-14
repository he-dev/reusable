using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public class PopulateExecution : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        var canPopulate =
            entry.TryGetProperty<MetaProperty.Scope, ILoggerScope>(out var scope) &&
            entry.TryGetProperty<MetaProperty.PopulateExecution>(out _);

        if (canPopulate)
        {
            if (scope.Items.TryGetValue("Exception", out var e) && e is Exception exception)
            {
                entry.Push(new LoggableProperty.Member("Faulted"));
                entry.Push(new LoggableProperty.Exception(exception));
            }
            else
            {
                entry.Push(new LoggableProperty.Member("Completed"));
            }
        }
        
        InvokeNext(entry);
    }
}