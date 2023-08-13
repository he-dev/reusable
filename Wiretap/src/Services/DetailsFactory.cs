using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.Wiretap.Services;

public static class DetailsFactory
{
    public static object CreateDetails(object? details)
    {
        return 
            details is not IEnumerable<KeyValuePair<string, object?>>
                ? details.EnumerateProperties().ToDictionary(x => x.Key, x => x.Value, SoftString.Comparer)
                : details;
    }
}