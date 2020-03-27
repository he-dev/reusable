using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Extensions;

namespace Reusable.Flowingo.Steps
{
    public class Require<T> : Step<T>
    {
        public IPredicate<T> That { get; set; }

        protected override Task<Flow> ExecuteBody(T context)
        {
            return (That.Invoke(context) ? Flow.Continue : Flow.Break).ToTask();
        }
    }
}