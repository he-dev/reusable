using System.Collections.Immutable;
using Reusable.IOnymous;
using Reusable.IOnymous.Config;

namespace Reusable
{
    internal class Dummy { }

    public static class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        public static readonly IResourceSquid Resources =
            ResourceSquid
                .Builder
                .AddController(new EmbeddedFileController<Dummy>(@"Reusable/res/IOnymous"))
                .AddController(new EmbeddedFileController<Dummy>(@"Reusable/res/Flexo"))
                .AddController(new EmbeddedFileController<Dummy>(@"Reusable/res/Utilities/JsonNet"))
                .AddController(new EmbeddedFileController<Dummy>(@"Reusable/sql"))
                .AddController(new AppSettingController())
                .AddController(new SqlServerController(ConnectionString)
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