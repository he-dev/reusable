using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Collections.Generic;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Helpers;

namespace Reusable.Flowingo.Steps
{
    public class Workflow<T> : Step<T>, IEnumerable<IStep<T>>
    {
        private readonly IStep<T> _first = Empty("First");

        public override async Task ExecuteAsync(T context)
        {
            try
            {
                await _first.ExecuteAsync(context);
            }
            catch (Exception inner)
            {
                //throw DynamicException.Create("ExecuteStep", $"An error occured wile executing '{step.GetType().ToPrettyString()}' [#{step.Tag}]. See inner exception for details.", inner);
            }


            await ExecuteNextAsync(context);
        }

        [DebuggerStepThrough]
        public virtual void Add(IStep<T> step) => _first.Tail().Append(step);

        /// <summary>
        /// Include/Exclude steps can be followed only by steps of the same kind. This API validates that this is the case.
        /// </summary>
        protected void ValidateStepPolicy<TFilterGroup>(IStep<T> step)
        {
            if (step is IFilterStep<TFilterGroup> filter)
            {
                var oppositePolicy = filter.Policy switch
                {
                    FilterPolicy.Include => FilterPolicy.Exclude,
                    FilterPolicy.Exclude => FilterPolicy.Include
                };

                // Are there already any filters of the same kind but with the opposite policy? This won't work...
                if (this.OfType<IFilterStep<TFilterGroup>>().Any(f => f.Group.Equals(filter.Group) && f.Policy == oppositePolicy))
                {
                    throw DynamicException.Create
                    (
                        "InconsistentFilterStep",
                        $"Cannot use {filter.Group} filter '{step.GetType().ToPrettyString()}' because the current policy is '{oppositePolicy}'."
                    );
                }
            }
        }

        public IEnumerator<IStep<T>> GetEnumerator() => _first.EnumerateNext().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this).GetEnumerator();
    }

    public enum FilterPolicy
    {
        Undefined,
        Include,
        Exclude,
    }

    public interface IFilterStep<out T>
    {
        T Group { get; }

        FilterPolicy Policy { get; }
    }
}