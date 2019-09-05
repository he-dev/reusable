using System;
using System.Collections.Immutable;
using Reusable.Data;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Converters;
using Reusable.Translucent.Middleware;

namespace Reusable
{
    public abstract class TestHelper
    {
        public static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        public static readonly IResourceRepository Resources = new ResourceRepository<TestResourceSetup>(default);

        private class TestResourceSetup
        {
            public void ConfigureServices(IResourceControllerBuilder controller)
            {
                controller.AddEmbeddedFiles<TestHelper>
                (
                    @"Reusable/res/IOnymous",
                    @"Reusable/res/Flexo",
                    @"Reusable/res/Utilities/JsonNet",
                    @"Reusable/sql"
                );
                controller.AddAppConfig();
                controller.AddSqlServer(
                    ConnectionString,
                    ImmutableContainer
                        .Empty
                        .SetItem(
                            SqlServerController.TableName,
                            ("reusable", "TestConfig"))
                        .SetItem(
                            SqlServerController.ColumnMappings,
                            ImmutableDictionary<SqlServerColumn, SoftString>
                                .Empty
                                .Add(SqlServerColumn.Name, "_name")
                                .Add(SqlServerColumn.Value, "_value"))
                        .SetItem(
                            SqlServerController.Where,
                            ImmutableDictionary<string, object>
                                .Empty
                                .Add("_env", "test")
                                .Add("_ver", "1"))
                        .SetItem(
                            SqlServerController.Fallback,
                            ImmutableDictionary<string, object>
                                .Empty
                                .Add("_env", "else")));
            }

            public void Configure(IResourceRepositoryBuilder repository)
            {
                repository.UseMiddleware<SettingFormatValidationMiddleware>();
                repository.UseMiddleware<SettingExistsValidationMiddleware>();
            }
        }
    }
}