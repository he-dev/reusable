using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus;
using Reusable.Utilities.Mailr.Models;
using Reusable.Translucent;
using Reusable.Translucent.Data;

namespace Reusable.Utilities.Mailr;

public static class ResourceHelpers
{
    public static async Task<string> SendEmailAsync(this IResource resource, string name, Email email, Action<HttpRequest>? configure = default)
    {
        using var response = await resource.CreateAsync<HttpRequest.Stream>(name, email, request =>
        {
            request.ControllerName = "Mailr";
            request.ContentType = "application/json";
            request.HeaderActions.Add(headers => { headers.AcceptHtml(); });
            request.Also(configure);
        });

        if (response.Body is Stream stream)
        {
            return await stream.ReadTextAsync();
        }
        else
        {
            throw new ArgumentException(paramName: nameof(response), message: $"Cannot deserialize resource '{response.ResourceName}' because it's not a stream.");
        }
    }
}