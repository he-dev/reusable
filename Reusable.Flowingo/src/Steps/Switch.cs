using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Helpers;

namespace Reusable.Flowingo.Steps
{
    public enum SwitchOption
    {
        FirstMatch,
        AllMatches,
    }

    public class Switch<T> : Step<T>, IEnumerable<IStep<T>>
    {
        private readonly List<IStep<T>> _steps = new List<IStep<T>>();

        public Switch(SwitchOption option = SwitchOption.FirstMatch)
        {
            Option = option;
        }

        public SwitchOption Option { get; set; }

        public void Add(IStep<T> step) => _steps.Add(step);

        public override async Task ExecuteAsync(T context)
        {
            var any = false;
            foreach (var step in _steps)
            {
                if (step is Case<T> @case)
                {
                    if (@case.When.Invoke(context))
                    {
                        //(context as ILoggerContext)?.Logger.LogInfo(this, $"Case '{step.GetType().ToPrettyString()}' matches the specified criteria #{step.Tag}.");
                        await @case.Then.ExecuteAsync(context);
                        if (Option == SwitchOption.FirstMatch)
                        {
                            break;
                        }

                        any = true;
                    }
                    else
                    {
                        //(context as ILoggerContext)?.Logger.LogInfo(this, $"Case '{step.GetType().ToPrettyString()}' doesn't match the specified criteria #{step.Tag}.");
                    }
                }
                else
                {
                    if (!any)
                    {
                        //(context as ILoggerContext)?.Logger.LogInfo(this, $"No case matches. Falling back to default '{step.GetType().ToPrettyString()}' #{step.Tag}.");
                        await step.ExecuteAsync(context);
                    }

                    break;
                }
            }

            await ExecuteNextAsync(context);
        }

        public IEnumerator<IStep<T>> GetEnumerator() => _steps.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_steps).GetEnumerator();
    }
}