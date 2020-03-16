using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.Flowingo.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Flowingo.Abstractions
{
    public interface IStep<T> : INode<IStep<T>>
    {
        bool Enabled { get; set; }

        object Tag { get; }

        Task ExecuteAsync(T context);
    }

    public abstract class Step<T> : IStep<T>
    {
        protected Step(ILogger logger)
        {
            Logger = logger;
        }

        public IStep<T>? Prev { get; set; }

        public IStep<T>? Next { get; set; }

        public bool Enabled { get; set; } = true;

        public object Tag { get; set; }

        protected ILogger Logger { get; }

        public virtual async Task ExecuteAsync(T context)
        {
            var canExecuteNext = false;
            try
            {
                using var scope = Logger.BeginScope().WithCorrelationHandle("ExecuteStep").UseStopwatch();
                canExecuteNext = await ExecuteBody(context);
                Logger.Log(Abstraction.Layer.Service().Routine(GetType().ToPrettyString()).Completed());
            }
            catch (Exception inner)
            {
                canExecuteNext = false;
                Logger.Log(Abstraction.Layer.Service().Routine(GetType().ToPrettyString()).Faulted(inner));
            }
            finally
            {
                if (canExecuteNext)
                {
                    await ExecuteNextAsync(context);
                }
            }
        }

        protected abstract Task<bool> ExecuteBody(T context);

        protected async Task ExecuteNextAsync(T context)
        {
            if (this.EnumerateNextWithoutSelf<IStep<T>>().FirstOrDefault(s => s.Enabled) is {} next)
            {
                await next.ExecuteAsync(context);
            }
        }

        public class Empty : Step<T>
        {
            public Empty() : base(default) { }

            public override Task ExecuteAsync(T context) => ExecuteNextAsync(context);

            protected override Task<bool> ExecuteBody(T context) => true.ToTask();
        }
    }
}