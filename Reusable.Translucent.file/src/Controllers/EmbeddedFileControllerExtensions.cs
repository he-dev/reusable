using System;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class EmbeddedFileControllerExtensions
    {
        public static IResourceCollection AddEmbeddedFile<T>(this IResourceCollection controllers, string? basePath = default, IImmutableContainer? properties = default)
        {
            return controllers.Add(new EmbeddedFileController<T>(basePath, properties));
        }

        public static IResourceCollection AddEmbeddedFile(this IResourceCollection controllers, Type assemblyProvider, string? basePath = default, IImmutableContainer? properties = default)
        {
            return controllers.Add(new EmbeddedFileController(assemblyProvider.Assembly, basePath, properties));
        }

        public static IResourceCollection AddPhysicalFile(this IResourceCollection controllers, string? basePath = default, IImmutableContainer? properties = default)
        {
            return controllers.Add(new PhysicalFileController(basePath, properties));
        }

        public static IResourceCollection UseInMemoryFiles<T>(this IResourceCollection controllers, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                controllers.Add(new EmbeddedFileController<T>(basePath));
            }

            return controllers;
        }
    }
}