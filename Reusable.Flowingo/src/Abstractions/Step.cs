using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.Flowingo.Data;
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
        protected Step()
        {
            Tag = this.CreateTag();
        }

        public IStep<T>? Prev { get; set; }

        public IStep<T>? Next { get; set; }

        public bool Enabled { get; set; } = true;

        public object Tag { get; set; }

        protected ILogger? Logger => AsyncScope<ILoggerFactory>.Current?.Value.CreateLogger(GetType().ToPrettyString());

        public virtual async Task ExecuteAsync(T context)
        {
            try
            {
                using var scope = Logger?.BeginScope().WithCorrelationHandle("ExecuteStep").UseStopwatch();
                Logger?.Log(Abstraction.Layer.Service().Step(new { Tag }, GetType().ToPrettyString()));
                var flow = await ExecuteBody(context).ContinueWith(t =>
                {
                    Logger?.Log(Abstraction.Layer.Service().Step(new { flow = t.Result }, GetType().ToPrettyString()), l => l.Exception(t.Exception));
                    return (t.Exception is null || t.Result == Flow.Continue) ? Flow.Continue : Flow.Break;
                });

                if (flow == Flow.Continue)
                {
                    await ExecuteNextAsync(context);
                }
            }
            catch (Exception inner)
            {
                Logger?.Log(Abstraction.Layer.Service().Routine(GetType().ToPrettyString()).Faulted(inner));
            }
        }

        protected abstract Task<Flow> ExecuteBody(T context);

        protected async Task ExecuteNextAsync(T context)
        {
            if (this.EnumerateNextWithoutSelf<IStep<T>>().FirstOrDefault(s => !(s is Continue) && s.Enabled) is {} next)
            {
                await next.ExecuteAsync(context);
            }
        }

        public class Continue : Step<T>
        {
            public Continue()
            {
                Tag = nameof(Continue).ToLower();
            }

            public override Task ExecuteAsync(T context) => ExecuteNextAsync(context);

            protected override Task<Flow> ExecuteBody(T context) => Flow.Continue.ToTask();
        }

        public class Break : Step<T>
        {
            public Break()
            {
                Tag = nameof(Continue).ToLower();
            }

            public override Task ExecuteAsync(T context) => ExecuteNextAsync(context);

            protected override Task<Flow> ExecuteBody(T context) => Flow.Break.ToTask();
        }
    }

    public static class StepHelper
    {
        public static string CreateTag<T>(this IStep<T> step)
        {
            return Regex.Replace(step.GetType().ToPrettyString(), @"(\<?[A-Z])", m =>
            {
                if (m.Index == 0)
                {
                    return m.Value;
                }

                return $"-{m.Value.Trim('<')}";
            }).ToLower();
        }
    }

    public enum StepState
    {
        Begin,
        End
    }
}