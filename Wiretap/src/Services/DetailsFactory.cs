using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Extensions;

namespace Reusable.Wiretap.Services;

public static class DetailsFactory
{
    public static object CreateDetails(object? details)
    {
        return 
            details is not IEnumerable<KeyValuePair<string, object?>>
                ? details.EnumerateProperties().ToImmutableDictionary(SoftString.Comparer)
                : details;
    }
}