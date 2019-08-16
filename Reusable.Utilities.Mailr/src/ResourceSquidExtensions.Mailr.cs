using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Utilities.Mailr.Models;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Formatting;

namespace Reusable.Utilities.Mailr
{
    public static class ResourceSquidExtensions
    {
        public static async Task<string> SendEmailAsync
        (
            this IResourceSquid resourceSquid,
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

            var response = await resourceSquid.InvokeAsync(new Request.Post(uri)
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