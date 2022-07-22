using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions;

public static class LoggerFactoryExtensions
{
    public static ILogger CreateLogger<T>(this ILoggerFactory loggerFactory) => loggerFactory.CreateLogger(typeof(T).ToPrettyString());
}