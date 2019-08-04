using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class Log : Dictionary<SoftString, object>, ILog
    {
        public Log() { }

        public Log(IDictionary<SoftString, object> source) : base(source) { }
        
        public static ILog Empty => new Log();
    }
}