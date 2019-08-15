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
                .UseEmbeddedFiles<TestHelper>
                (
                    @"Reusable/res/IOnymous",
                    @"Reusable/res/Flexo",
                    @"Reusable/res/Utilities/JsonNet",
                    @"Reusable/sql"
                )
                .UseAppConfig()
                .UseSqlServer(ConnectionString, server =>
                {
                    server.TableName = ("reusable", "TestConfig");
                    server.ResourceConverter = new JsonSettingConverter();
                    server.ColumnMappings =
                        ImmutableDictionary<SqlServerColumn, SoftString>
                            .Empty
                            .Add(SqlServerColumn.Name, "_name")
                            .Add(SqlServerColumn.Value, "_value");
                    server.Where =
                        ImmutableDictionary<string, object>
                            .Empty
                            .Add("_env", "test")
                            .Add("_ver", "1");
                    server.Fallback = ("_env", "else");
                })
                .Build();
    }
}