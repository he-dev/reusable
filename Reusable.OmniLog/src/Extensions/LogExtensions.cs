using System;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Extensions
{
    public static class LogExtensions
    {
        #region Log properties

        public static LogEntry Logger(this LogEntry logEntry, string value) => logEntry.SetItem(nameof(Logger), default, value);
        
        public static LogEntry Timestamp(this LogEntry logEntry, DateTime value) => logEntry.SetItem(nameof(Timestamp), default, value);
        
        public static LogEntry Level(this LogEntry logEntry, LogLevel value) => logEntry.SetItem(nameof(Level), default, value);
        
        public static LogEntry Exception(this LogEntry logEntry, Exception value) => logEntry.SetItem(nameof(Exception), default, value);
        
        public static LogEntry Message(this LogEntry logEntry, string value) => logEntry.SetItem(nameof(Message), default, value);

        #endregion

//        #region With
//
//        public static Log WithCallerInfo
//        (
//            this Log log,
//            [CallerMemberName] string callerMemberName = null,
//            [CallerLineNumber] int callerLineNumber = 0,
//            [CallerFilePath] string callerFilePath = null
//        )
//        {
//            log.Add(Log.PropertyNames.CallerMemberName, callerMemberName);
//            log.Add(Log.PropertyNames.CallerLineNumber, callerLineNumber);
//            log.Add(Log.PropertyNames.CallerFilePath, callerFilePath);
//
//            return log;
//        }
//
//        #endregion
    }
}