using System;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Controllers;

public abstract class ConfigService<T> : Service where T : IRequest
{
    protected ConfigService() => MustSucceed = true;

    public override async Task<object> InvokeAsync(IRequest request)
    {
        if (request is not T setting)
        {
            throw DynamicException.Create("Request", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(ConfigService<T>)}.");
        }

        return await InvokeAsync(setting) switch
        {
            Unit unit => MustSucceed ? throw new Exception() : unit,
            { } result => result
        };
    }

    protected abstract Task<object> InvokeAsync(T setting);
}