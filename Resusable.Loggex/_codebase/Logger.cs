using Reusable.Logging.Collections;
using Reusable.Logging.ComputedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging
{
    public interface ILogger
    {
        string Name { get; }
        void Log(LogEntry logEntry);
        bool CanLog(LogEntry logEntry);
    }

    public abstract class Logger : ILogger
    {
        public static ComputedPropertyCollection ComputedProperties { get; } = new ComputedPropertyCollection();

        protected Logger(string name) => Name = name;

        public string Name { get; }

        public void Log(LogEntry logEntry)
        {
            if (CanLog(logEntry ?? throw new ArgumentNullException(nameof(logEntry)))) LogCore(ComputeProperties());

            logEntry.SetValue(nameof(ILogger.Name), Name);

            LogEntry ComputeProperties() => new LogEntry(logEntry.Concat((Dictionary<string, object>)ComputedProperties).ToDictionary(
                x => x.Key,
                x => x.Value is IComputedProperty p ? p.Compute(logEntry) : x.Value,
                StringComparer.OrdinalIgnoreCase
            ));
        }

        protected abstract void LogCore(LogEntry logEntry);

        public abstract bool CanLog(LogEntry logEntry);
    }

    public class NullLogger : ILogger
    {
        public string Name => string.Empty;

        public bool CanLog(LogEntry logEntry) => false;

        public void Log(LogEntry logEntry) { }
    }

    public static class LoggerExtensions
    {

    }
}