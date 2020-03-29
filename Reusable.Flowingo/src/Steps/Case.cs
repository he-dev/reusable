using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;
using Reusable.Flowingo.Helpers;

namespace Reusable.Flowingo.Steps
{
    public class Case<T> : Step<T>
    {
        [Required]
        public IPredicate<T> When { get; set; } = default!;

        [Required]
        public IStep<T> Then { get; set; } = default!;

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