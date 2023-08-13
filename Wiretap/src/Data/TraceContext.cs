using System.Collections.Generic;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Data;

public class TraceContext
{
    public required ActivityContext Activity { get; init; }
    public IDictionary<string, object?> Entry { get; } = new SoftDictionary();
    public IDictionary<string, object?> Items { get; } = new SoftDictionary();
}