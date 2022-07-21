using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions;

public static class LoggerFactoryExtensions
{
    public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory) => new Logger<T>(loggerFactory);
}