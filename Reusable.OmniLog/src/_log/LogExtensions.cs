using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static Log Logger(this Log log, string value) => log.SetItem((nameof(Logger), default), value);
        
        public static Log Timestamp(this Log log, DateTime value) => log.SetItem((nameof(Timestamp), default), value);
        
        public static Log Level(this Log log, LogLevel value) => log.SetItem((nameof(Level), default), value);
        
        public static Log Exception(this Log log, Exception value) => log.SetItem((nameof(Exception), default), value);
        
        public static Log Message(this Log log, string value) => log.SetItem((nameof(Message), default), value);

        //public static Log Transform(this Log log, Func<Log, Log> transform) => transform(log);

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