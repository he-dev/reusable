using System;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class JsonFileControllerExtensions
    {
        public static IResourceCollection AddJsonFile(this IResourceCollection controllers, string? id, string basePath, string fileName, Action<JsonFileController>? controllerAction = default)
        {
            var controller = new JsonFileController(id, basePath, fileName);
            controllerAction?.Invoke(controller);
            return controllers.Add(controller);
        }
    }
}