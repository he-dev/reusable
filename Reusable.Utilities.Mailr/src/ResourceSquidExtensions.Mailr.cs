using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Utilities.Mailr.Models;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Formatting;

namespace Reusable.Utilities.Mailr
{
    public static class ResourceSquidExtensions
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
            var properties =
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
                    .UpdateItem(ResourceController.Tags, tags => tags.Add(controllerTag.ToSoftString()));

            var response = await resourceRepository.InvokeAsync(new Request.Post(uri)
            {
                Metadata = properties,
                Body = email
            });

            using (response)
            {
                return await response.DeserializeTextAsync();
            }
        }
    }
}