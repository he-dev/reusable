using Reusable.OmniLog.Abstractions;

namespace Reusable.Flowingo.Abstractions
{
    public interface ILoggerContext
    {
        ILogger Logger { get; }
    }
}