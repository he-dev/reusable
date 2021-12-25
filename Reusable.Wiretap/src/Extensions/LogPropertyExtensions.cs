using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions;

public static class LogPropertyExtensions
{
    public static T ValueOrDefault<T>(this ILogProperty? property, T fallback) => property?.Value switch { T t => t, _ => fallback };
}