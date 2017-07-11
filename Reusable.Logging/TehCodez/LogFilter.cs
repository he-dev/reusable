using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Loggex
{
    public interface ILogFilter
    {
        ISet<CaseInsensitiveString> Loggers { get; }

        ISet<CaseInsensitiveString> Recorders { get; }

        LogLevel LogLevel { get; }

        bool Stop { get; }

        bool IsMatch(ILogger logger, IRecorder recorder, LogEntry logEntry);
    }

    public class LogFilter : ILogFilter
    {
        public ISet<CaseInsensitiveString> Loggers { get; set; } = new HashSet<CaseInsensitiveString>();

        public ISet<CaseInsensitiveString> Recorders { get; set; } = new HashSet<CaseInsensitiveString>();

        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        public bool Stop { get; set; } = true;

        public bool IsMatch(ILogger logger, IRecorder recorder, LogEntry logEntry)
        {
            return
                (!Loggers.Any() || Loggers.Contains(logger.Name)) &&
                Recorders.Contains(recorder.Name) &&
                LogLevel >= logEntry.LogLevel();
        }
    }
}