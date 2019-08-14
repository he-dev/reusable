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
using Reusable.IOnymous;
using Reusable.Utilities.Mailr.Models;
using Reusable.IOnymous.Controllers;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Formatting;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.Utilities.Mailr
{
    // Provides CRUD APIs.
    public static class ResourceSquidExtensions
    {
        public static async Task<string> SendEmailAsync
        (
            this IResourceSquid resourceSquid,
            UriString uri,
            UserAgent userAgent,
            Email email,
            string providerName
        )
        {
            var properties =
                ImmutableContainer
                    .Empty
                    .SetItem(HttpRequestMetadata.ConfigureHeaders, headers =>
                    {
                        headers
                            .UserAgent(userAgent.ProductName, userAgent.ProductVersion)
                            .AcceptHtml();
                    })
                    .SetItem(HttpRequestMetadata.ContentType, "application/json")
                    .SetItem(HttpResponseMetadata.Formatters, new[] { new TextMediaTypeFormatter() })
                    .SetItem(HttpResponseMetadata.ContentType, "application/json")
                    .UpdateItem(ResourceControllerProperties.Tags, tags => tags.Add(providerName.ToSoftString()));

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