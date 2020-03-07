using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Flowingo.Abstractions;

namespace Reusable.Flowingo.Steps
{
    public class Lambda<T> : Step<T>
    {
        private readonly Func<T, Task<bool>> _execute;

        public Lambda(Func<T, Task<bool>> execute) => _execute = execute;

        public override async Task ExecuteAsync(T context)
        {
            if (await _execute.Invoke(context))
            {
                await ExecuteNextAsync(context);
            }
        }
    }
}