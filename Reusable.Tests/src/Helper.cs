using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Rx;
using Reusable.OmniLog.Services;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable
{
    [UsedImplicitly]
    public class TestHelperFixture : IDisposable
    {
        public TestHelperFixture()
        {
            Logs = new MemoryRx();
            LoggerFactory = TestHelper.CreateLoggerFactory(Logs);
            Resources =
                ResourceRepository
                    .From<TestResourceResourceSetup>(
                        ImmutableServiceProvider
                            .Empty
                            .Add(TestHelper.CreateCache())
                            .Add(LoggerFactory));
        }

        public MemoryRx Logs { get; }

        public ILoggerFactory LoggerFactory { get; }

        public IResourceRepository Resources { get; }

        public void Dispose()
        {
            LoggerFactory.Dispose();
        }
    }

    public abstract class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";
        
        public static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

        public static ILoggerFactory CreateLoggerFactory(ILogRx logRx)
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
                        (LogEntry.Names.SnapshotName, "Identifier")
                    )
                    .UseFallback((LogEntry.Names.Level, LogLevel.Information))
                    .UseEcho(logRx)
                    .Build();
        }
    }

    [UsedImplicitly]
    internal class TestResourceResourceSetup : ResourceRepositorySetup
    {
        public override IEnumerable<IResourceController> Controllers(IServiceProvider services)
        {
            var assembly = typeof(TestHelper).Assembly;

            yield return new EmbeddedFileController(ControllerName.Empty, @"Reusable/res/Beaver", assembly);
            yield return new EmbeddedFileController(ControllerName.Empty, @"Reusable/res/Translucent", assembly);
            yield return new EmbeddedFileController(ControllerName.Empty, @"Reusable/res/Flexo", assembly);
            yield return new EmbeddedFileController(ControllerName.Empty, @"Reusable/res/Utilities/JsonNet", assembly);
            yield return new EmbeddedFileController(ControllerName.Empty, @"Reusable/sql", assembly);
            yield return new AppSettingController(ControllerName.Empty);
            yield return new SqlServerController(ControllerName.Empty, TestHelper.ConnectionString)
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

        public override IEnumerable<IMiddlewareInfo<ResourceContext>> Middleware(IServiceProvider services)
        {
            yield return Use<CacheMiddleware>();
            yield return Use<SettingValidationMiddleware>();
            yield return Use<ResourceExistsValidationMiddleware>();
        }
    }
}