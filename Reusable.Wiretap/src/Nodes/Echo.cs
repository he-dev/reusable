using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node passes the received entry to the specified <c>Connectors</c>. It's usually the last node.
/// </summary>
public class Echo : LoggerNode
{
    public override bool Enabled => Connectors?.Any() == true;

    public List<IConnector> Connectors { get; set; } = new();
    
    public override void Invoke(ILogEntry entry)
    {
        var view = new LogEntryView<ILoggableProperty>(entry);

        foreach (var rx in Connectors)
        {
            rx.Log(view);
        }

        InvokeNext(entry);
    }
}