using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Extensions;
using Reusable.Wiretap.AspNetCore.Abstractions;
using Reusable.Wiretap.AspNetCore.Services;

namespace Reusable.Wiretap.AspNetCore;

public static class WiretapExtensions
{
    /// <summary>
    /// Adds Wiretap services to the container.
    /// </summary>
    public static IServiceCollection AddWiretap(this IServiceCollection services)
    {
        return
            services
                .AddSingleton<ITakeSnapshot<HttpRequest>, TakeRequestSnapshot>()
                .AddSingleton<ITakeSnapshot<HttpResponse>, TakeResponseSnapshot>()
                .AddSingleton<ISerialize<HttpRequest>, SerializeRequest>()
                .AddSingleton<ISerialize<HttpResponse>, SerializeResponse>();
    }
    
    /// <summary>
    /// Registers Wiretap middleware.
    /// </summary>
    public static IApplicationBuilder UseWiretap(this IApplicationBuilder builder, Action<WiretapMiddleware.Configuration> configure = default)
    {
        return builder.UseMiddleware<WiretapMiddleware>(new WiretapMiddleware.Configuration().Also(configure));
    }
}