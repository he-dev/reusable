using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flowingo.Steps
{
    public class InitializeLogger<T> : Step<T>
    {
        public InitializeLogger(ILoggerFactory loggerFactory) => AsyncScope<ILoggerFactory>.Push(loggerFactory);

        protected override Task<Flow> ExecuteBody(T context) => Flow.Continue.ToTask();
    }
}