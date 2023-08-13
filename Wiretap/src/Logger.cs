using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public class Logger : ILogger
{
    public required LogFunc Log { get; init; }

    [MustUseReturnValue]
    public ActivityContext LogBegin
    (
        string name,
        string? message = default,
        object? details = default,
        object? attachment = default
    )
    {
        return new ActivityContext { Name = name, Log = Log }.Also(activity => activity.LogBegin(message, details, attachment));
    }
}