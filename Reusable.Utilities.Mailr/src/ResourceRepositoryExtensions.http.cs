using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Utilities.Mailr.Models;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Formatting;

namespace Reusable.Utilities.Mailr
{
    public static class ResourceRepositoryExtensions
    {
        public static async Task<string> SendEmailAsync
        (
            this IResourceRepository resourceRepository,
            UriString uri,
            UserAgent userAgent,
            Email email,
            string controllerTag = "Mailr"
        )
        {
            var metadata =
                ImmutableContainer
                    .Empty
                    .SetItem(HttpRequest.ConfigureHeaders, headers =>
                    {
                        headers
                            .UserAgent(userAgent.ProductName, userAgent.ProductVersion)
                            .AcceptHtml();
                    })
                    .SetItem(HttpRequest.ContentType, "application/json")
                    .SetItem(HttpResponse.Formatters, new[] { new TextMediaTypeFormatter() })
                    .SetItem(HttpResponse.ContentType, "application/json")
                    .SetItem(ResourceController.Schemes, UriSchemes.Known.Http, UriSchemes.Known.Https)
                    .UpdateItem(ResourceController.Tags, tags => tags.Add(controllerTag.ToSoftString()));

            using var response = await resourceRepository.PostAsync(uri, email, metadata);
            return await response.DeserializeTextAsync();
        }
    }
}