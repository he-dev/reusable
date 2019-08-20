using System;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class ResourceSquidBuilderExtensions
    {
        public static IResourceControllerBuilder AddEmbeddedFiles<T>(this IResourceControllerBuilder builder, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                builder.Controllers.Add(new EmbeddedFileController<T>(basePath));
            }

            return builder;
        }

        public static IResourceControllerBuilder AddEmbeddedFiles(this IResourceControllerBuilder builder, Type assemblyProvider, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                builder.Controllers.Add(new EmbeddedFileController(assemblyProvider.Assembly, ImmutableContainer.Empty.SetItem(EmbeddedFileControllerProperties.BaseUri, basePath)));
            }

            return builder;
        }

        public static IResourceControllerBuilder AddPhysicalFiles(this IResourceControllerBuilder builder, string basePath = default)
        {
            builder
                .Controllers
                .Add(
                    basePath is null
                        ? new PhysicalFileController()
                        : new PhysicalFileController(basePath));

            return builder;
        }

        public static IResourceControllerBuilder AddPhysicalFiles(this IResourceControllerBuilder builder, IImmutableContainer properties)
        {
            return builder.AddController(new PhysicalFileController(properties));
        }

        public static IResourceControllerBuilder UseInMemoryFiles<T>(this IResourceControllerBuilder builder, params string[] basePaths)
        {
            foreach (var basePath in basePaths)
            {
                builder.Controllers.Add(new EmbeddedFileController<T>(basePath));
            }

            return builder;
        }
    }
}