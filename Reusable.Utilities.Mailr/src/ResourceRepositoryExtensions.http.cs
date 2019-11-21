using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Utilities.Mailr.Models;
using Reusable.Translucent;

namespace Reusable.Utilities.Mailr
{
    public static class ResourceRepositoryExtensions
    {
        public static async Task<string> SendEmailAsync
        (
            this IResourceRepository resourceRepository,
            UriString uri,
            Email email,
            Action<HttpRequest>? requestAction = default
        )
        {
            using var response = await resourceRepository.HttpInvokeAsync(RequestMethod.Post, uri, email, request =>
            {
                request.ControllerTags = new HashSet<SoftString> { "Mailr" };
                request.ConfigureHeaders = headers => headers.AcceptHtml();
                requestAction?.Invoke(request);
            });
            return await response.DeserializeTextAsync();
        }
    }
}