using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class SettingValidationMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;

        public SettingValidationMiddleware(RequestDelegate<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request.Method.In(RequestMethod.Post, RequestMethod.Put))
            {
                Validate(context.Request.Uri, context.Request.Body, context.Request.Metadata.GetItemOrDefault(Setting.Validations));
            }

            await _next(context);

            if (context.Request.Method == RequestMethod.Get)
            {
                Validate(context.Request.Uri, context.Response.Body, context.Request.Metadata.GetItemOrDefault(Setting.Validations));
            }
        }

        private static void Validate(UriString uri, object? obj, IEnumerable<ValidationAttribute>? validations)
        {
            if (!uri.Scheme.Equals(ConfigController.Scheme))
            {
                return;
            }

            foreach (var validation in validations ?? Enumerable.Empty<ValidationAttribute>())
            {
                try
                {
                    validation.Validate(obj, new ValidationContext(obj));
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create("SettingValidation", $"Setting '{uri}' does not pass the '{validation.GetType().ToPrettyString()}' validation. See the inner exception for details.", inner);
                }
            }
        }
    }
}