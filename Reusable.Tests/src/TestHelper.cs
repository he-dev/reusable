using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Caching.Memory;
using Reusable.OmniLog;
using Reusable.Translucent;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Nodes;
using Reusable.Translucent.ResourceValidations;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Services.Properties;

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
                LoggerPipelines
                    .Complete
                    .Configure<InvokePropertyService>(node =>
                    {
                        node.Services.Add(new Constant("Environment", "Test"));
                        node.Services.Add(new Constant("Product", "Reusable.Tests"));
                        node.Services.Add(new Timestamp<DateTimeUtc>());
                    })
                    .Configure<Echo>(node => node.Connectors.AddRange(logRx))
                    .ToLoggerFactory();
        }

        public static IResource CreateResource(IMemoryCache? memoryCache = default)
        {
            var assembly = typeof(TestHelper).Assembly;

            return new Resource(new IResourceNode[]
            {
                new CacheInMemory(memoryCache ?? new MemoryCache(new MemoryCacheOptions())),
                new ValidateRequiredResource(new CompositeResourceValidator
                {
                    new RequestMethodNotNone(),
                    new ResourceNameNotNullOrEmpty(),
                    new RequiredResourceExists(),
                    new ValidateSetting()
                }),
                new ProcessRequest(new IResourceController[]
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