using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Commander.Commands
{
    public class CommandTelemetry : CommandDecorator
    {
        private readonly ILogger<CommandTelemetry> _logger;

        public CommandTelemetry(ILogger<CommandTelemetry> logger, ICommand command) : base(command)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope().WithCorrelationHandle("ExecuteCommand").UseStopwatch())
            {
                _logger.Log(Abstraction.Layer.Service().Subject(new { CommandName = Command.Name.First() }).Trace());
                try
                {
                    await Command.ExecuteAsync(parameter, cancellationToken);
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Completed());
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Canceled());
                    throw;
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Faulted(), taskEx);
                    throw;
                }
            }
        }
    }
}