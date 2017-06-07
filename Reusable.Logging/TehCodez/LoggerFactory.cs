using System;

namespace Reusable.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);
    }

    public static class LoggerFactory
    {
        private static ILoggerFactory _loggerFactory;

        public static void Initialize<T>() where T : ILoggerFactory, new() => _loggerFactory = new T();

        public static ILogger CreateLogger(string name) => (_loggerFactory ?? throw new InvalidOperationException($"{nameof(LoggerFactory)} is not initialized.")).CreateLogger(name);
    }
}
