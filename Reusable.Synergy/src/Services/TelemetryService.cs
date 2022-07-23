using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Synergy.Services;

[PublicAPI]
public class TelemetryService : Service
{
    public TelemetryService(ILogger<TelemetryService> logger) => Logger = logger;

    public ILogger<TelemetryService> Logger { get; }

    public override async Task<object> InvokeAsync(IRequest request)
    {
        using var scope = Logger.BeginScope(request.GetType().ToPrettyString());
        try
        {
            return await InvokeNext(request);
        }
        catch (Exception ex)
        {
            scope.Exception(ex);
            throw;
        }
        finally
        {
            Logger.Log(Telemetry.Collect.Application().UnitOfWork(nameof(InvokeAsync)).Auto());
        }
    }
}