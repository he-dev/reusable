using System;
using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Conventions;

public static class Decision
{
    public static IEnumerable<Action<ILogEntry>> Make(string decision, string? because = default)
    {
        yield return log => log.Layer(nameof(TelemetryLayers.Application));
        yield return log => log.Category(nameof(TelemetryCategories.Logic));
        yield return log => log.Unit(nameof(TelemetryUnits.Decision), decision);
        yield return log => log.Message(because);
    }
}