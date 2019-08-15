using System.Collections.Immutable;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Converters;

namespace Reusable
{
    public abstract class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        public static readonly IResourceSquid Resources =
            ResourceSquid
                .Builder
                .UseController(new EmbeddedFileController<TestHelper>(@"Reusable/res/IOnymous"))
                .UseController(new EmbeddedFileController<TestHelper>(@"Reusable/res/Flexo"))
                .UseController(new EmbeddedFileController<TestHelper>(@"Reusable/res/Utilities/JsonNet"))
                .UseController(new EmbeddedFileController<TestHelper>(@"Reusable/sql"))
                .UseController(new AppSettingController())
                .UseController(new SqlServerController(ConnectionString)
                {
                    TableName = ("reusable", "TestConfig"),
                    ResourceConverter = new JsonSettingConverter(),
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
                    Fallback = ("_env", "else")
                })
                .Build();
    }
}