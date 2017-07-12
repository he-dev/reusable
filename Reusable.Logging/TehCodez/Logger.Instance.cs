using System;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Logging.Loggex
{
    public partial class Logger
    {
        private readonly LoggerConfiguration configuration;

        private Logger(CaseInsensitiveString name, LoggerConfiguration configuration)
        {
            Name = name;
            this.configuration = configuration;
        }

        public CaseInsensitiveString Name { get; }

        public ILogger Log([NotNull] LogEntry logEntry)
        {
            if (logEntry == null) throw new ArgumentNullException(nameof(logEntry));

            logEntry.SetValue(nameof(ILogger.Name), Name);

            foreach (var property in configuration.ComputedProperties)
            {
                logEntry[property.Name] = property.Compute(logEntry);
            }

            var message = logEntry.Message().Format(logEntry);
            logEntry.Message(message);

            var matches =
                from r in configuration.Recorders
                from f in configuration.Filters
                where f.IsMatch(this, r, logEntry)
                select (Recorder: r, Filter: f);

            foreach (var t in matches)
            {
                t.Recorder.Log(logEntry);
                if (t.Filter.Stop)
                {
                    break;
                }
            }

            return this;
        }
    }
}
