using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging
{
    public static class LogEntryExtensions
    {       
        public static LogEntry LogLevel(this LogEntry entry, LogLevel logLevel) { entry[nameof(LogLevel)] = logLevel; return entry; }

        public static LogEntry Trace(this LogEntry entry) => entry.LogLevel(Logging.LogLevel.Trace);
        public static LogEntry Debug(this LogEntry entry) => entry.LogLevel(Logging.LogLevel.Debug);
        public static LogEntry Info(this LogEntry entry) => entry.LogLevel(Logging.LogLevel.Info);
        public static LogEntry Warn(this LogEntry entry) => entry.LogLevel(Logging.LogLevel.Warn);
        public static LogEntry Error(this LogEntry entry) => entry.LogLevel(Logging.LogLevel.Error);
        public static LogEntry Fatal(this LogEntry entry) => entry.LogLevel(Logging.LogLevel.Fatal);

        public static LogEntry MessageBuilder(this LogEntry entry, Action<StringBuilder> builder)
        {
            builder(entry.GetValueOrCreate(nameof(Message), () => new StringBuilder()));
            return entry;
        }

        public static LogEntry Message(this LogEntry entry, string message) => entry.MessageBuilder(b => b.Clear().Append(message));

        public static StringBuilder MessageBuilder(this LogEntry entry) => entry.GetValueOrCreate(nameof(Message), () => new StringBuilder());

        public static LogEntry Exception(this LogEntry entry, Exception exception) => entry.SetValue(nameof(Exception), exception);

        public static LogEntry Stopwatch(this LogEntry entry, Action<Stopwatch> stopwatch)
        {
            stopwatch(entry.GetValueOrCreate(nameof(Stopwatch), () => new Stopwatch()));
            return entry;
        }

        public static LogEntry Caller(this LogEntry entry, [CallerMemberName] string callerName = null) => entry.SetValue(nameof(Caller), callerName);

        public static LogEntry LineNumber(this LogEntry entry, [CallerLineNumber] int lineNumber = 0) => entry.SetValue(nameof(LineNumber), lineNumber);

        public static void Log(this LogEntry entry, ILogger logger) => logger.Log(entry);

        public static T GetValueOrCreate<T>(this LogEntry entry, string name, Func<T> create)
        {
            return entry.TryGetValue(name, out object value) ? (T)value : (T)(entry[name] = (value = create()));
        }

        public static T GetValue<T>(this LogEntry entry, string name) => entry.TryGetValue(name, out object value) ? (T)value : default(T);

        public static LogEntry SetValue<T>(this LogEntry entry, string name, T value)
        {
            (entry ?? throw new ArgumentNullException(nameof(entry)))[name] = value;
            return entry;
        }

        public static LogEntry SetValue(this LogEntry entry, IComputedProperty property) => entry.SetValue(property.Name, property);

        public static AutoLogEntry AsAutoLog(this LogEntry logEntry, ILogger logger) => new AutoLogEntry(logEntry, logger);

    }
}
