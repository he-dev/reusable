using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Flowingo.Abstractions;

namespace Reusable.Flowingo.Steps
{
    public class Require<T> : Step<T>
    {
        public IPredicate<T> That { get; set; }

        public override async Task ExecuteAsync(T context)
        {
            if (That.Invoke(context))
            {
                await ExecuteNextAsync(context);
            }
            else
            {
                //context.Logger.LogInfo(this, $"Context does not meet the requirement #{Tag}.");
            }
        }
    }
}