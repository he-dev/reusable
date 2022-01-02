using System.Collections.Generic;

namespace Reusable.Synergy;

public static class ServiceExtensions
{
    // Enumerates service pipeline.
    public static IEnumerable<IService> Enumerate(this IService service)
    {
        for (var current = service; current is { }; current = current.Next)
        {
            yield return current;
        }
    }
}