using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Caching.Memory;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Properties;
using Reusable.Translucent;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;
using Reusable.Translucent.Middleware.ResourceValidator;
using Reusable.Translucent.Middleware.ResourceValidators;

namespace Reusable
{
    public abstract class TestHelper
    {
        // Seeing passwords, huh? These are just debug containers.
        // public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";
        public static readonly string ConnectionString = "Data Source=localhost;Initial Catalog=TestDb;User Id=SA;Password=ABC123!!!;"; // this is just a debug

        public static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

        public static ILoggerFactory CreateLoggerFactory(params IConnector[] logRx)
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
                        (Names.Default.SnapshotName, "Identifier")
                    )
                    .UseFallback((Names.Default.Level, LogLevel.Information))
                    .UseEcho(logRx)
                    .Build();
        }

        public static IResource CreateResource(IMemoryCache? memoryCache = default)
        {
            var assembly = typeof(TestHelper).Assembly;

            return new Resource(new IResourceMiddleware[]
            {
                new ResourceMemoryCache(memoryCache ?? new MemoryCache(new MemoryCacheOptions())),
                new ResourceValidation(new CompositeResourceValidator
                {
                    new RequestMethodNotNone(),
                    new ResourceNameNotNullOrEmpty(),
                    new RequiredResourceExists(),
                    new SettingAttributeValidator()
                }),
                new ResourceSearch(new IResourceController[]
                {
                    new EmbeddedFileController(@"Reusable/res/Beaver", assembly),
                    new EmbeddedFileController(@"Reusable/res/Translucent", assembly),
                    new EmbeddedFileController(@"Reusable/res/Flexo", assembly),
                    new EmbeddedFileController(@"Reusable/res/Utilities/JsonNet", assembly),
                    new EmbeddedFileController(@"Reusable/sql", assembly),
                    new AppSettingController(),
                    new SqlServerController<TestSetting>(ConnectionString, name => setting => setting.Name == name && setting.Environment.Equals("test") && setting.Version.Equals("1")),
                    new SqlServerController<TestSetting>(ConnectionString, name => setting => setting.Name == name && setting.Environment.Equals("else") && setting.Version.Equals("1")),
                }),
            });
        }

        [Table("TestConfig", Schema = "reusable")]
        public class TestSetting : ISetting
        {
            [Column("_id")]
            public int Id { get; set; }

            [Column("_name")]
            public string Name { get; set; }

            [Column("_value")]
            public string Value { get; set; }

            [Column("_env")]
            public string Environment { get; set; }

            [Column("_ver")]
            public string Version { get; set; }
        }
    }
}