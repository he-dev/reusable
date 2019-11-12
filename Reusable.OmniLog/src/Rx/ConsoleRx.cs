using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Rx.ConsoleRenderers;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Rx
{
    public interface IConsoleRenderer
    {
        void Render(LogEntry logEntry);
    }
    
    [PublicAPI]
    public class ConsoleRx : ILogRx
    {
        public IConsoleRenderer Renderer { get; set; } = new SimpleConsoleRenderer();

        public void Log(LogEntry logEntry)
        {
            Renderer.Render(logEntry);
        }
    }
}