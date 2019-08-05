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
    public class LogLevel : Option<LogLevel>
    {
        public LogLevel(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static LogLevel Trace { get; } = CreateWithCallerName();
        public static LogLevel Debug { get; } = CreateWithCallerName();
        public static LogLevel Information { get; } = CreateWithCallerName();
        public static LogLevel Warning { get; } = CreateWithCallerName();
        public static LogLevel Error { get; } = CreateWithCallerName();
        public static LogLevel Fatal { get; } = CreateWithCallerName();
    }
}