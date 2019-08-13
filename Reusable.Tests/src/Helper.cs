using System.Collections.Immutable;
using Reusable.IOnymous;
using Reusable.IOnymous.Config;

namespace Reusable
{
    internal class Dummy { }

    public static class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        public static readonly IResourceRepository Resources = new ResourceRepository(builder =>
        {
            builder.UseResources
            (
                new EmbeddedFileProvider<Dummy>(@"Reusable/res/IOnymous"),
                new EmbeddedFileProvider<Dummy>(@"Reusable/res/Flexo"),
                new EmbeddedFileProvider<Dummy>(@"Reusable/res/Utilities/JsonNet"),
                new EmbeddedFileProvider<Dummy>(@"Reusable/sql"),
                new AppSettingProvider(),
                new SqlServerProvider(ConnectionString)
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
                }
            );
        });
    }
}