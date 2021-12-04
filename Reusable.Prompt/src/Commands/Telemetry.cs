using System;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;

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
            using (_logger.BeginScope("ExecuteCommand", new { commandName = Decoratee.Name.Primary }))
            {
                try
                {
                    await Decoratee.ExecuteAsync(parameter, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Scope().Exceptions.Push(ex);
                    throw;
                }
            }
        }
    }
}