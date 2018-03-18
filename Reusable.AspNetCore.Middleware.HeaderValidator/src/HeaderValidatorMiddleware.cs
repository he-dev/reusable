using System;
using System.Collections.Generic;
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

        private readonly IEnumerable<SoftString> _requiredHeaders;

        public HeaderValidatorMiddleware(RequestDelegate next, IEnumerable<string> requiredHeaders)
        {
            _next = next;
            _requiredHeaders = requiredHeaders.Select(SoftString.Create).ToList();
        }

        public async Task Invoke(HttpContext context)
        {
            var missingHeaders = 
                _requiredHeaders
                    .Except(context.Request.Headers.Keys.Select(SoftString.Create))
                    .ToList();

            if (missingHeaders.Any())
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
        public static IApplicationBuilder UseHeaderValidator(this IApplicationBuilder builder, params string[] requiredHeaders)
        {
            return builder.UseMiddleware<HeaderValidatorMiddleware>(requiredHeaders.ToList());
        }
    }
}
