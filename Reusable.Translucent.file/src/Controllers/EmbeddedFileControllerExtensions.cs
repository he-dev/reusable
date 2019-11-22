using System;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class EmbeddedFileControllerExtensions
    {
        public static IResourceCollection AddEmbeddedFile<T>(this IResourceCollection controllers, string? id, string? basePath = default)
        {
            return controllers.Add(new EmbeddedFileController<T>(id, basePath));
        }

        public static IResourceCollection AddEmbeddedFile(this IResourceCollection controllers, string? id, Type assemblyProvider, string? basePath = default)
        {
            return controllers.Add(new EmbeddedFileController(id, basePath, assemblyProvider.Assembly));
        }

        public static IResourceCollection AddPhysicalFile(this IResourceCollection controllers, string? id, string? basePath = default)
        {
            return controllers.Add(new PhysicalFileController(id, basePath));
        }

        public static IResourceCollection UseInMemoryFile(this IResourceCollection controllers, string? id)
        {
            return controllers.Add(new InMemoryFileController(id));
        }
    }
}