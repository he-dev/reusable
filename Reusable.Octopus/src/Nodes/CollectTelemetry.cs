using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Octopus.Nodes;

[PublicAPI]
[UsedImplicitly]
public class CollectTelemetry : ResourceNode
{
    public CollectTelemetry(ILogger<CollectTelemetry> logger) => Logger = logger;

    private ILogger Logger { get; }

    public Func<ResourceContext, bool> Filter { get; set; } = _ => true;

    public override async Task InvokeAsync(ResourceContext context)
    {
        if (Filter(context))
        {
            await InvokeNextWithTelemetry(context);
        }
        else
        {
            await InvokeNext(context);
        }
    }

    private async Task InvokeNextWithTelemetry(ResourceContext context)
    {
        using var scope = Logger.BeginScope("ResourceRequest");
        try
        {
            Logger.Log(Telemetry.Collect.Application().Execution().Started(new
            {
                method = context.Request.Method.ToString().ToUpper(),
                resourceName = context.Request.ResourceName,
                controllerFilter = context.Request.ControllerFilter?.GetType().ToPrettyString(),
                items = context.Request.Items,
            }));

            await InvokeNext(context);
        }
        catch (Exception inner)
        {
            scope.Exception(inner);
            throw;
        }
        finally
        {
            Logger.Log(Telemetry.Collect.Application().Execution().Auto().Message(context.Response?.StatusCode.ToString()));
        }
    }
}