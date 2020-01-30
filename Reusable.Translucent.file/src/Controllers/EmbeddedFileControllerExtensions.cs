using System;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class EmbeddedFileControllerExtensions
    {
        public static IResourceCollection AddEmbeddedFile<T>(this IResourceCollection controllers, ControllerName controllerName, string? basePath = default)
        {
            return controllers.Add(new EmbeddedFileController<T>(controllerName, basePath));
        }

        public static IResourceCollection AddEmbeddedFile(this IResourceCollection controllers, ControllerName controllerName, string basePath, Type assemblyProvider)
        {
            return controllers.Add(new EmbeddedFileController(controllerName, basePath ?? assemblyProvider.Namespace, assemblyProvider.Assembly));
        }

        public static IResourceCollection AddPhysicalFile(this IResourceCollection controllers, ControllerName controllerName, string? basePath = default)
        {
            return controllers.Add(new PhysicalFileController(controllerName, basePath));
        }

        public static IResourceCollection UseInMemoryFile(this IResourceCollection controllers, ControllerName controllerName)
        {
            return controllers.Add(new InMemoryFileController(controllerName));
        }
    }
}