using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Utilities.Mailr.Models;
using Reusable.Translucent;
using Reusable.Translucent.Extensions;

namespace Reusable.Utilities.Mailr
{
    public static class ResourceRepositoryExtensions
    {
        public static async Task<string> SendEmailAsync
        (
            this IResource resource,
            UriString uri,
            Email email,
            Action<HttpRequest>? configureRequest = default
        )
        {
            using var response = await resource.PostAsync<HttpRequest>(uri, email, request =>
            {
                request.ControllerName = "Mailr";
                request.ContentType = "application/json";
                request.HeaderActions.Add(headers => { headers.AcceptHtml(); });
                configureRequest?.Invoke(request);
            });
            return await response.DeserializeTextAsync();
        }
    }
}