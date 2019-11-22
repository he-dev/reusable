using System;
using System.Net.Http;
using Reusable.Translucent.Controllers;
using Reusable.Extensions;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class HttpControllerExtensions
    {
        public static IResourceCollection AddHttp(this IResourceCollection controllers, string? id, string baseUri, Action<HttpController>? configureController = default)
        {
            return controllers.Add(HttpController.FromBaseUri(id, baseUri).Pipe(configureController));
        }

        public static IResourceCollection AddHttp(this IResourceCollection controllers, string? id, HttpClient httpClient, Action<HttpController>? configureController = default)
        {
            return controllers.Add(new HttpController(id, httpClient).Pipe(configureController));
        }
    }
}