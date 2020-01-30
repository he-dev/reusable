using System;
using System.Net.Http;
using Reusable.Translucent.Controllers;
using Reusable.Extensions;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class HttpControllerExtensions
    {
        public static IResourceCollection AddHttp(this IResourceCollection controllers, ControllerName controllerName, string baseUri, Action<HttpController>? configureController = default)
        {
            return controllers.Add(HttpController.FromBaseUri(controllerName, baseUri).Pipe(configureController));
        }

        public static IResourceCollection AddHttp(this IResourceCollection controllers, ControllerName controllerName, HttpClient httpClient, Action<HttpController>? configureController = default)
        {
            return controllers.Add(new HttpController(controllerName, httpClient).Pipe(configureController));
        }
    }
}