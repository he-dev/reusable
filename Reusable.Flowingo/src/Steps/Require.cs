using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Flowingo.Steps
{
    public class Require<T> : Step<T>
    {
        public IPredicate<T> That { get; set; }

        protected override Task<Flow> ExecuteBody(T context)
        {
            if (That.Invoke(context) is var canExecuteNext && canExecuteNext)
            {
                Logger?.Log(Abstraction.Layer.Service().Flow().Decision("Do not execute next step.").Because($"The required condition #{Tag} is not met."));
            }
            else
            {
                Logger?.Log(Abstraction.Layer.Service().Flow().Decision("Execute next step.").Because($"The required condition #{Tag} is met."));
            }

            return (canExecuteNext ? Flow.Continue : Flow.Break).ToTask();
        }
    }
}