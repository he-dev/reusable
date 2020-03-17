using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;

namespace Reusable.Flowingo.Steps
{
    public class Lambda<T> : Step<T>
    {
        private readonly Func<T, Task<bool>> _execute;

        public Lambda(Func<T, Task<bool>> execute) => _execute = execute;

        protected override async Task<Flow> ExecuteBody(T context)
        {
            return await _execute.Invoke(context) ? Flow.Continue : Flow.Break;
        }
    }
}