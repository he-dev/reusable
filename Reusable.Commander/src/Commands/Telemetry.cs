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
    public class Telemetry : CommandDecorator
    {
        private readonly ILogger<Telemetry> _logger;

        public Telemetry(ILogger<Telemetry> logger, ICommand command) : base(command)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            using var commandScope = _logger.BeginScope().WithCorrelationHandle("ExecuteCommand");
            _logger.Log(Application.Context.Service().WorkItem("Command", new { commandName = Decoratee.Name.Primary }));
            try
            {
                await Decoratee.ExecuteAsync(parameter, cancellationToken);
            }
            catch (Exception taskEx)
            {
                _logger.Scope().WorkItem().Push(taskEx);
                throw;
            }
        }
    }
}