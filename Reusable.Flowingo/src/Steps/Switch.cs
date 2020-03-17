using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Helpers;

namespace Reusable.Flowingo.Steps
{
    public class Switch<T> : Workflow<T>
    {
        public override void Add(IStep<T> step)
        {
            if (!(step is Case<T>) && (this.LastOrDefault() is Case<T>))
            {
                throw new InvalidOperationException("You can add only one default step that must be preceded by at least one case.");
            }

            base.Add(step);
        }
    }
}