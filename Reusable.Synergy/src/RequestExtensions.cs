using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy;

public static class RequestExtensions
{
    public static async Task<T> InvokeAsync<T>(this IRequest<T> request, IComponentContext components, CancellationToken cancellationToken = default)
    {
        using (request.Also(r => r.CancellationToken = cancellationToken))
        {
            var node =
                components.ResolveOptionalNamed<Service.PipelineBuilder>(request.Tag()) is { } builder
                    ? builder.Build()
                    : throw DynamicException.Create("PipelineNotFound", $"There is no pipeline to invoke {request.GetType().ToPrettyString()}");
            return
                await node.InvokeAsync(request) is T result
                    ? result
                    : throw DynamicException.Create("Request", $"{request.GetType().ToPrettyString()} did not return any result.");
        }
    }
}