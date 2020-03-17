using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;
using Reusable.Flowingo.Helpers;

namespace Reusable.Flowingo.Steps
{
    public class Case<T> : Step<T>
    {
        public IPredicate<T> When { get; set; }

        public IStep<T> Then { get; set; }

        public bool Break { get; set; } = true;

        protected override async Task<Flow> ExecuteBody(T context)
        {
            if (When.Invoke(context))
            {
                await Then.ExecuteAsync(context);
                return Break ? Flow.Break : Flow.Continue;
            }

            return Flow.Break;
        }
    }
    
    
}