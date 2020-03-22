using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public class Logger : LoggerNode, ILogger
    {
        public string Name { get; set; }

        public override void Invoke(ILogEntry request)
        {
            request.Push(Names.Default.Logger, Name, LogProperty.Process.With<EchoNode>());
            InvokeNext(request);
        }

        public virtual void Log(ILogEntry logEntry)
        {
            Invoke(logEntry);
        }
    }

    public class Logger<T> : Logger, ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(T).ToPrettyString());
        }

        public static ILogger<T> Empty { get; } = new EmptyLogger();

        public override ILoggerNode? Prev
        {
            get => _logger.Prev;
            set => _logger.Prev = value;
        }

        public override ILoggerNode? Next
        {
            get => _logger.Next;
            set => _logger.Next = value;
        }

        public override void Log(ILogEntry logEntry) => _logger.Log(logEntry);

        private class EmptyLogger : Logger, ILogger<T> { }
    }
}