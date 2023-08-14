using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public static class LoggerExtensions
{
    [MustUseReturnValue]
    public static IActivity Begin(this ILogger logger, string name, string? message = default, object? details = default, object? attachment = default)
    {
        return new ActivityContext(name, logger).Also(activity => activity.LogBegin(message, details, attachment));
    }
}