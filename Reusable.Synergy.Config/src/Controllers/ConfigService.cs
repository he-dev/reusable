using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Controllers;

public abstract class ConfigService<T> : Service where T : IRequest
{
    protected ConfigService() => MustSucceed = true;

    public override async Task<object> InvokeAsync(IRequest request)
    {
        var setting = ThrowIfNot<T>(request);
        
        return await InvokeAsync(setting) switch
        {
            Unit unit => MustSucceed ? Failure(request, "Could not...") : unit,
            { } result => result
        };
    }

    protected abstract Task<object> InvokeAsync(T setting);
}