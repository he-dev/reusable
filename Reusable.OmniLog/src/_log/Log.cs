using System.Collections.Generic;
using Reusable.Collections;

namespace Reusable.OmniLog
{
    public interface ILog : IDictionary<SoftString, object> { }

    public class Log : Dictionary<SoftString, object>, ILog { }    
}