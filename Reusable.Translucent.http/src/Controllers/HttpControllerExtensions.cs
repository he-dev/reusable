using System;
using System.Net.Http;
using Reusable.Translucent.Controllers;
using Reusable.Extensions;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class HttpControllerExtensions
    {
        public static IResourceCollection AddHttp(this IResourceCollection controllers, ComplexName name, string baseUri, Action<HttpController>? configureController = default)
        {
            return controllers.Add(HttpController.FromBaseUri(name, baseUri).Pipe(configureController));
        }

        public static IResourceCollection AddHttp(this IResourceCollection controllers, ComplexName name, HttpClient httpClient, Action<HttpController>? configureController = default)
        {
            return controllers.Add(new HttpController(name, httpClient).Pipe(configureController));
        }
    }
}