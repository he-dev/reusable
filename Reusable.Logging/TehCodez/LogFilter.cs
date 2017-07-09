using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Loggex
{
    public interface ILogFilter
    {
        ISet<CaseInsensitiveString> Loggers { get; set; }

        ISet<CaseInsensitiveString> Recorders { get; set; }

        LogLevel LogLevel { get; set; }

        bool OnMatchGoToNext { get; set; }

        bool IsMatch(ILogger logger, IRecorder recorder, LogEntry logEntry);
    }

    public class LogFilter : ILogFilter
    {
        public ISet<CaseInsensitiveString> Loggers { get; set; } = new HashSet<CaseInsensitiveString>();

        public ISet<CaseInsensitiveString> Recorders { get; set; } = new HashSet<CaseInsensitiveString>();

        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        public bool OnMatchGoToNext { get; set; } = true;

        public bool IsMatch(ILogger logger, IRecorder recorder, LogEntry logEntry)
        {
            return
                (!Loggers.Any() || Loggers.Contains(logger.Name)) &&
                Recorders.Contains(recorder.Name) &&
                LogLevel >= logEntry.LogLevel;
        }
    }
}