using System.Collections.Generic;
using Reusable.Collections;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class Log : Dictionary<SoftString, object>, ILog { }    
}