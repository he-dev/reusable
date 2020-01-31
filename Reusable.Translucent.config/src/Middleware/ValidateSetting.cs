using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Validates settings on their way in/out using their validation attributes.
    /// </summary>
    [UsedImplicitly]
    public class ValidateSetting : MiddlewareBase
    {
        public ValidateSetting(RequestDelegate<ResourceContext> next, IServiceProvider services) : base(next, services) { }

        public override async Task InvokeAsync(ResourceContext context)
        {
            var configRequest = context.Request as ConfigRequest;

            if (context.Request.Method.In(RequestMethod.Post, RequestMethod.Put) && configRequest is {})
            {
                Validate(context.Request.Uri, context.Request.Body, configRequest.ValidationAttributes);
            }

            await InvokeNext(context);

            if (context.Request.Method == RequestMethod.Get && configRequest is {})
            {
                Validate(context.Request.Uri, context.Response.Body, configRequest.ValidationAttributes);
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