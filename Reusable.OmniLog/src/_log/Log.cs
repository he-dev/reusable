using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class Log : Dictionary<SoftString, object>, ILog
    {
        public static ILog Empty => new Log();
    }    
}