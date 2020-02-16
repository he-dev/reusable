using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Services;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;
using Reusable.Translucent.Middleware.ResourceValidator;
using Reusable.Translucent.Middleware.ResourceValidators;

namespace Reusable
{
    public abstract class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        public static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

        public static ILoggerFactory CreateLoggerFactory(params ILogRx[] logRx)
        {
            return
                LoggerFactory
                    .Builder()
                    .UseStopwatch()
                    .UseService
                    (
                        new Constant("Environment", "Test"),
                        new Constant("Product", "Reusable.Tests"),
                        new Timestamp<DateTimeUtc>()
                    )
                    .UseDelegate()
                    .UseScope()
                    .UseBuilder()
                    .UseDestructure()
                    .UseObjectMapper()
                    .UseSerializer()
                    .UsePropertyMapper
                    (
                        (LogProperty.Names.SnapshotName, "Identifier")
                    )
                    .UseFallback((LogProperty.Names.Level, LogLevel.Information))
                    .UseEcho(logRx)
                    .Build();
        }

        public static IResource CreateResource()
        {
            var assembly = typeof(TestHelper).Assembly;

            return
                Resource
                    .Builder()
                    .UseController(new EmbeddedFileController(@"Reusable/res/Beaver", assembly))
                    .UseController(new EmbeddedFileController(@"Reusable/res/Translucent", assembly))
                    .UseController(new EmbeddedFileController(@"Reusable/res/Flexo", assembly))
                    .UseController(new EmbeddedFileController(@"Reusable/res/Utilities/JsonNet", assembly))
                    .UseController(new EmbeddedFileController(@"Reusable/sql", assembly))
                    .UseController(new AppSettingController())
                    .UseController(new SqlServerController(ConnectionString)
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
                    })
                    .UseMiddleware(services => next => new ResourceMemoryCache(next, services.GetService<IMemoryCache>() ?? new MemoryCache(new MemoryCacheOptions())))
                    .UseMiddleware(services => next => new ResourceValidation(next, new CompositeResourceValidator
                    {
                        new RequestMethodNotNone(),
                        new ResourceNameNotNullOrEmpty(),
                        new RequiredResourceExists(),
                        new SettingAttributeValidator()
                    }))
                    .Build(ImmutableServiceProvider.Empty.Add(CreateCache()).Add(CreateLoggerFactory()));
        }
    }
}