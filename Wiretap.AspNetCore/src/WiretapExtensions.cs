using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
                .AddSingleton<IFilter<HttpRequest>, RequestFilter>()
                .AddSingleton<IFilter<HttpResponse>, ResponseFilter>();
    }
    
    /// <summary>
    /// Registers Wiretap middleware.
    /// </summary>
    public static IApplicationBuilder UseWiretap(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WiretapMiddleware>();
    }
}