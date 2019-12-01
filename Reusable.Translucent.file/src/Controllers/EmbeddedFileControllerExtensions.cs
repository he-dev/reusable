using System;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class EmbeddedFileControllerExtensions
    {
        public static IResourceCollection AddEmbeddedFile<T>(this IResourceCollection controllers, ComplexName name, string? basePath = default)
        {
            return controllers.Add(new EmbeddedFileController<T>(name, basePath));
        }

        public static IResourceCollection AddEmbeddedFile(this IResourceCollection controllers, ComplexName name, string basePath, Type assemblyProvider)
        {
            return controllers.Add(new EmbeddedFileController(name, basePath ?? assemblyProvider.Namespace, assemblyProvider.Assembly));
        }

        public static IResourceCollection AddPhysicalFile(this IResourceCollection controllers, ComplexName name, string? basePath = default)
        {
            return controllers.Add(new PhysicalFileController(name, basePath));
        }

        public static IResourceCollection UseInMemoryFile(this IResourceCollection controllers, ComplexName name)
        {
            return controllers.Add(new InMemoryFileController(name));
        }
    }
}