using System;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.Tests.Foggle;
using Xunit;

namespace Reusable.Tests.SmartConfig.Providers
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
            var c = CompositeProvider.Empty.Add(new SqlServerProvider("name=TestDb")
            {
                TableName = ("reusable", "SmartConfig"),
                ValueConverter = new JsonSettingConverter(),
                ColumnMappings =
                    ImmutableDictionary<SqlServerColumn, SoftString>
                        .Empty
                        .Add(SqlServerColumn.Name, "_name")
                        .Add(SqlServerColumn.Value, "_value")
            });
            var name = await c.ReadSettingAsync(From<IUserConfig>.Select(x => x.Name));
            var isCool = await c.ReadSettingAsync(From<IUserConfig>.Select(x => x.IsCool));

            Assert.Equal("Bob", name);
            Assert.True(isCool);
        }

        [Fact]
        public async Task Can_get_setting_with_additional_criteria()
        {
            var c = CompositeProvider.Empty.Add(new SqlServerProvider("name=TestDb")
            {
                TableName = ("reusable", "SmartConfig"),
                ValueConverter = new JsonSettingConverter(),
                ColumnMappings =
                    ImmutableDictionary<SqlServerColumn, SoftString>
                        .Empty
                        .Add(SqlServerColumn.Name, "_name")
                        .Add(SqlServerColumn.Value, "_value"),
                Where = ImmutableDictionary<string, object>.Empty.Add("_other", "someone-else")
            });
            var name = await c.ReadSettingAsync(From<IUserConfig>.Select(x => x.Name));

            Assert.Equal("Tom", name);
        }

        [Fact]
        public async Task Can_deserialize_various_types()
        {
            var c = CompositeProvider.Empty.Add(new SqlServerProvider("name=TestDb")
            {
                TableName = ("reusable", "SmartConfig"),
                ValueConverter = new JsonSettingConverter(),
                ColumnMappings =
                    ImmutableDictionary<SqlServerColumn, SoftString>
                        .Empty
                        .Add(SqlServerColumn.Name, "_name")
                        .Add(SqlServerColumn.Value, "_value"),
                Where =
                    ImmutableDictionary<string, object>
                        .Empty
                        .Add("_other", "t"),
                Fallback = ("_other", "something-else")
            });

            var types = From<ITypeConfig>.This;

            Assert.Equal("str", await c.ReadSettingAsync(types.Select(x => x.String)));
            Assert.Equal(true, await c.ReadSettingAsync(types.Select(x => x.Bool)));
            Assert.Equal(3, await c.ReadSettingAsync(types.Select(x => x.Int)));
            Assert.Equal(1.25, await c.ReadSettingAsync(types.Select(x => x.Double)));
            Assert.Equal(new DateTime(2019, 1, 2), await c.ReadSettingAsync(types.Select(x => x.DateTime)));
            Assert.Equal(new[] { 3, 4, 5 }, await c.ReadSettingAsync(types.Select(x => x.ListOfInt)));
            Assert.Equal(TimeSpan.FromMinutes(20), await c.ReadSettingAsync(types.Select(x => x.TimeSpan)));
        }

        [Fact]
        public async Task Can_save_setting()
        {
            var c = CompositeProvider.Empty.Add(new SqlServerProvider("name=TestDb")
            {
                TableName = ("reusable", "SmartConfig"),
                ValueConverter = new JsonSettingConverter(),
                ColumnMappings =
                    ImmutableDictionary<SqlServerColumn, SoftString>
                        .Empty
                        .Add(SqlServerColumn.Name, "_name")
                        .Add(SqlServerColumn.Value, "_value"),
                Where = ImmutableDictionary<string, object>.Empty.Add("_other", "t")
            });

            Assert.Equal(7, await c.ReadSettingAsync(From<ITypeConfig>.Select(x => x.Edit)));

            await c.WriteSettingAsync(From<ITypeConfig>.Select(x => x.Edit), 12);

            Assert.Equal(12, await c.ReadSettingAsync(From<ITypeConfig>.Select(x => x.Edit)));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [UseType, UseMember]
        [SettingSelectorFormatter]
        [TrimStart("I"), TrimEnd("Config")]
        private interface IUserConfig : INamespace
        {
            string Name { get; }

            bool IsCool { get; }
        }
    }
}