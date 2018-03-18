using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Reusable.Extensions;

namespace Reusable.AspNetCore.Middleware
{
    public class HeaderValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IImmutableSet<SoftString> _requiredHeaders;
        private readonly IImmutableSet<SoftString> _excludedPaths;

        public HeaderValidatorMiddleware(RequestDelegate next, IEnumerable<string> requiredHeaders, IEnumerable<string> excludedPaths)
        {
            _next = next;
            _requiredHeaders = requiredHeaders.Select(SoftString.Create).ToImmutableHashSet();
            _excludedPaths = excludedPaths.Select(SoftString.Create).ToImmutableHashSet();
        }

        public async Task Invoke(HttpContext context)
        {
            // Ignore default route from mandatory headers.
            var isExcludedPath = context.Request.Path.HasValue && _excludedPaths.Contains(context.Request.Path.Value); // == "/api/home")
            var missingHeaders = _requiredHeaders.Where(name => !context.Request.Headers.ContainsKey(name.ToString())).ToList();
            if (missingHeaders.Any() && !isExcludedPath)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"One or more required headers are missing: {missingHeaders.Select(header => header.ToString()).Join(", ").EncloseWith("[]")}");
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class HeaderValidatorMiddlewareExtensions
    {
        //public static IApplicationBuilder UseHeaderValidator(this IApplicationBuilder builder, IEnumerable<string> requiredHeaders, IEnumerable<string> excludedPaths)
        //{
        //    return builder.UseMiddleware<HeaderValidatorMiddleware>(requiredHeaders, excludedPaths);
        //}

        public static IApplicationBuilder UseHeaderValidator(this IApplicationBuilder builder, IEnumerable<string> requiredHeaders)
        {
            return builder.UseMiddleware<HeaderValidatorMiddleware>(requiredHeaders, Enumerable.Empty<string>());
        }
    }
}
