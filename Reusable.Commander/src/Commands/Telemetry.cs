using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

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
            using var commandScope = _logger.BeginScope("ExecuteCommand");
            _logger.Log(Execution.Context.Service().WorkItem("Command", new { commandName = Decoratee.Name.Primary }));
            try
            {
                await Decoratee.ExecuteAsync(parameter, cancellationToken);
            }
            catch (Exception taskEx)
            {
                _logger.Scope().Exceptions.Push(taskEx);
                throw;
            }
        }
    }
}