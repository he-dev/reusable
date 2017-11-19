using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public class LoggerConfiguration
    {
        [NotNull]
        public HashSet<ILogAttachement> Attachements { get; set; } = new HashSet<ILogAttachement>();

        [NotNull]
        public LoggerPredicate LoggerPredicate { get; set; } = OmniLog.LoggerFilter.Any;
        
        [NotNull]
        public LogPredicate LogPredicate { get; set; } = OmniLog.LogFilter.Any;
    }
}