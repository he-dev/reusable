using Reusable.Logging.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;

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
            if (CanLog(logEntry ?? throw new ArgumentNullException(nameof(logEntry))))
            {
                logEntry.SetValue(nameof(ILogger.Name), Name);

                foreach (var property in (Dictionary<string, object>)ComputedProperties)
                {
                    logEntry[property.Key] = ((IComputedProperty)property.Value).Compute(logEntry);
                }

                var message = logEntry.MessageBuilder().ToString().FormatAll(logEntry);
                logEntry.Message(message);

                LogCore(logEntry);
            }            
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
}