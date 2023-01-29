using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Wiretap.Utilities.AspNetCore.Mvc.Filters;
using Reusable.Wiretap.Utilities.AspNetCore.Services;

namespace Reusable.Wiretap.Utilities.AspNetCore;

public static class WiretapExtensions
{
    /// <summary>
    /// Adds Wiretap services to the container.
    /// </summary>
    public static IServiceCollection AddWiretap(this IServiceCollection services)
    {
        return
            services
                .AddScoped<TakeRequestSnapshot>()
                .AddScoped<TakeResponseSnapshot>()
                .AddScoped<SerializeRequestBody>()
                .AddScoped<CanLogResponseBody>();
    }
    
    /// <summary>
    /// Registers Wiretap middleware.
    /// </summary>
    public static IApplicationBuilder UseWiretap(this IApplicationBuilder builder, WiretapMiddleware.Configuration? config = default)
    {
        return builder.UseMiddleware<WiretapMiddleware>(config ?? new WiretapMiddleware.Configuration());
    }
}