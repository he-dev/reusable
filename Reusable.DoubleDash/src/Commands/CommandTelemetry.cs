using System;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;

namespace Reusable.DoubleDash.Commands;

public class CommandTelemetry : CommandDecorator
{
    private readonly ILogger<CommandTelemetry> _logger;

    public CommandTelemetry(ILogger<CommandTelemetry> logger, ICommand command) : base(command)
    {
        _logger = logger;
    }

    public override async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
    {
        using (_logger.BeginScope("ExecuteCommand"))
        {
            _logger.Log(Telemetry.Collect.Application().Metadata(new { commandName = Decoratee.NameCollection.Primary }));
            _logger.Log(Telemetry.Collect.Application().UnitOfWork(nameof(ExecuteAsync)).Started());
            try
            {
                await Decoratee.ExecuteAsync(parameter, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Scope().Exception(ex);
                throw;
            }
            finally
            {
                _logger.Log(Telemetry.Collect.Application().UnitOfWork(nameof(ExecuteAsync)).Auto());
            }
        }
    }
}