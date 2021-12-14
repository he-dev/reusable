using System;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Commander.Commands;

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
            _logger.Log(Telemetry.Collect.Application().Execution().Started(new { commandName = Decoratee.Name.Primary }));
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
                _logger.Log(Telemetry.Collect.Application().Execution().Auto());
            }
        }
    }
}