using Microsoft.Extensions.Caching.Memory;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Services;

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
    }
}