using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;
using Reusable.Translucent.Middleware.ResourceValidator;
using Reusable.Translucent.Middleware.ResourceValidators;

namespace Reusable
{
    [UsedImplicitly]
    internal class TestResourceFactory
    {
        public static IEnumerable<CreateControllerDelegate> CreateControllers(IServiceProvider services)
        {
            var assembly = typeof(TestHelper).Assembly;

            yield return () => new EmbeddedFileController(ControllerName.Any, @"Reusable/res/Beaver", assembly);
            yield return () => new EmbeddedFileController(ControllerName.Any, @"Reusable/res/Translucent", assembly);
            yield return () => new EmbeddedFileController(ControllerName.Any, @"Reusable/res/Flexo", assembly);
            yield return () => new EmbeddedFileController(ControllerName.Any, @"Reusable/res/Utilities/JsonNet", assembly);
            yield return () => new EmbeddedFileController(ControllerName.Any, @"Reusable/sql", assembly);
            yield return () => new AppSettingController(ControllerName.Any);
            yield return () => new SqlServerController(ControllerName.Any, TestHelper.ConnectionString)
            {
                TableName = ("reusable", "TestConfig"),
                ColumnMappings =
                    ImmutableDictionary<SqlServerColumn, SoftString>
                        .Empty
                        .Add(SqlServerColumn.Name, "_name")
                        .Add(SqlServerColumn.Value, "_value"),
                Where =
                    ImmutableDictionary<string, object>
                        .Empty
                        .Add("_env", "test")
                        .Add("_ver", "1"),
                Fallback =
                    ImmutableDictionary<string, object>
                        .Empty
                        .Add("_env", "else")
            };
        }

        public static IEnumerable<CreateMiddlewareDelegate> CreateMiddleware(IServiceProvider services)
        {
            yield return next => new ResourceMemoryCache(next, services.GetService<IMemoryCache>() ?? new MemoryCache(new MemoryCacheOptions()));
            yield return next => new ResourceValidation(next, new CompositeResourceValidator
            {
                new RequestMethodNotNone(),
                new ResourceNameNotNullOrEmpty(),
                new RequiredResourceExists(),
                new SettingAttributeValidator()
            });
        }
    }
}