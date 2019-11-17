using System;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class EmbeddedFileControllerExtensions
    {
        public static IResourceCollection AddEmbeddedFiles<T>(this IResourceCollection controllers, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                controllers.Add(new EmbeddedFileController<T>(basePath));
            }

            return controllers;
        }

        public static IResourceCollection AddEmbeddedFiles(this IResourceCollection controllers, Type assemblyProvider, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                controllers.Add(new EmbeddedFileController(assemblyProvider.Assembly, basePath));
            }

            return controllers;
        }

        public static IResourceCollection AddPhysicalFiles(this IResourceCollection controllers, string? basePath = default, IImmutableContainer? properties = default)
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