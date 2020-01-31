using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

namespace Reusable
{
    [UsedImplicitly]
    internal class TestResourceSetup : ResourceRepositorySetup
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

        public override IEnumerable<IMiddlewareInfo> Middleware(IServiceProvider services)
        {
            yield return Use<MemoryCache>();
            yield return Use<ValidateSetting>();
            yield return Use<ValidateResourceExists>();
        }
    }
}