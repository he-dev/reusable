using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public abstract class LogLevel
    {
        public static Option<LogLevel> Trace { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Debug { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Information { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Warning { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Error { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Fatal { get; } = Option<LogLevel>.CreateWithCallerName();
    }
}