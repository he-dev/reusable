using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Utilities.Mailr.Models;
using Reusable.Translucent;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Utilities.Mailr
{
    public static class ResourceHelpers
    {
        public static async Task<string> SendEmailAsync
        (
            this IResource resource,
            string uri,
            Email email,
            Action<HttpRequest>? configureRequest = default
        )
        {
            using var response = await resource.CreateAsync<HttpRequest>(uri, email, request =>
            {
                request.ControllerName = "Mailr";
                request.ContentType = "application/json";
                request.HeaderActions.Add(headers => { headers.AcceptHtml(); });
                request.Also(configureRequest);
            });
            return await response.DeserializeTextAsync();
        }
    }
}