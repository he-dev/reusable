using System;
using System.Collections.Generic;
using Reusable.Data;
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
            return controllers.Add(new EmbeddedFileController(id, assemblyProvider.Assembly, basePath));
        }

        public static IResourceCollection AddPhysicalFile(this IResourceCollection controllers, string? id, string? basePath = default)
        {
            return controllers.Add(new PhysicalFileController(id, basePath));
        }

        public static IResourceCollection UseInMemoryFiles<T>(this IResourceCollection controllers, string? id, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                controllers.Add(new EmbeddedFileController<T>(id, basePath));
            }

            return controllers;
        }
    }
}