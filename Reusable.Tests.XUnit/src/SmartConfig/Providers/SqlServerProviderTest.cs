using System;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.Utilities.SqlClient;
using Xunit;

namespace Reusable.Tests.XUnit.SmartConfig.Providers
{
    public class SqlServerProviderTest : IAsyncLifetime
    {
        private static readonly IResourceProvider Sql =
            EmbeddedFileProvider<SqlServerProviderTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"sql\SmartConfig"));

        public async Task InitializeAsync()
        {
            var connectionString = ConnectionStringRepository.Default.GetConnectionString("name=TestDb");
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(Sql.ReadTextFile("seed-test-data.sql"));
            }
        }

        [Fact]
        public async Task Can_get_setting()
        {
            var c = new Configuration<IUserConfig>(new CompositeProvider(new[]
            {
                new SqlServerProvider("name=TestDb")
                {
                    TableName = ("reusable", "SmartConfig"),
                    ValueConverter = new JsonSettingConverter(),
                    ColumnMappings =
                        ImmutableDictionary<SqlServerColumn, SoftString>
                            .Empty
                            .Add(SqlServerColumn.Name, "_name")
                            .Add(SqlServerColumn.Value, "_value")
                }
            }));
            var name = await c.GetItemAsync(x => x.Name);
            var isCool = await c.GetItemAsync(x => x.IsCool);

            Assert.Equal("Bob", name);
            Assert.True(isCool);
        }

        [Fact]
        public async Task Can_get_setting_with_additional_criteria()
        {
            var c = new Configuration<IUserConfig>(new CompositeProvider(new[]
            {
                new SqlServerProvider("name=TestDb")
                {
                    TableName = ("reusable", "SmartConfig"),
                    ValueConverter = new JsonSettingConverter(),
                    ColumnMappings =
                        ImmutableDictionary<SqlServerColumn, SoftString>
                            .Empty
                            .Add(SqlServerColumn.Name, "_name")
                            .Add(SqlServerColumn.Value, "_value"),
                    Where = ImmutableDictionary<string, object>.Empty.Add("_other", "Someone-else")
                },
            }));
            var name = await c.GetItemAsync(x => x.Name);

            Assert.Equal("Tom", name);
        }

        [Fact]
        public async Task Can_deserialize_various_types()
        {
            var c = new Configuration<ITypeConfig>(new CompositeProvider(new[]
            {
                new SqlServerProvider("name=TestDb")
                {
                    TableName = ("reusable", "SmartConfig"),
                    ValueConverter = new JsonSettingConverter(),
                    ColumnMappings =
                        ImmutableDictionary<SqlServerColumn, SoftString>
                            .Empty
                            .Add(SqlServerColumn.Name, "_name")
                            .Add(SqlServerColumn.Value, "_value"),
                    Where = ImmutableDictionary<string, object>.Empty.Add("_other", "t")
                },
            }));

            Assert.Equal("str", await c.GetItemAsync(x => x.String));
            Assert.Equal(true, await c.GetItemAsync(x => x.Bool));
            Assert.Equal(3, await c.GetItemAsync(x => x.Int));
            Assert.Equal(1.25, await c.GetItemAsync(x => x.Double));
            Assert.Equal(new DateTime(2019, 1, 2), await c.GetItemAsync(x => x.DateTime));
            Assert.Equal(new[] { 3, 4, 5 }, await c.GetItemAsync(x => x.ListOfInt));
            Assert.Equal(TimeSpan.FromMinutes(20), await c.GetItemAsync(x => x.TimeSpan));
        }

        [Fact]
        public async Task Can_save_setting()
        {
            var c = new Configuration<ITypeConfig>(new CompositeProvider(new[]
            {
                new SqlServerProvider("name=TestDb")
                {
                    TableName = ("reusable", "SmartConfig"),
                    ValueConverter = new JsonSettingConverter(),
                    ColumnMappings =
                        ImmutableDictionary<SqlServerColumn, SoftString>
                            .Empty
                            .Add(SqlServerColumn.Name, "_name")
                            .Add(SqlServerColumn.Value, "_value"),
                    Where = ImmutableDictionary<string, object>.Empty.Add("_other", "t")
                },
            }));

            Assert.Equal(7, await c.GetItemAsync(x => x.Edit));

            await c.SetItemAsync(x => x.Edit, 12);

            Assert.Equal(12, await c.GetItemAsync(x => x.Edit));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private interface IUserConfig
        {
            string Name { get; }

            bool IsCool { get; }
        }
    }
}