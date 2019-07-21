using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Reusable.Data.Repositories;
using Reusable.IOnymous;
using Reusable.IOnymous.Config;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Tests.IOnymous.Config.Providers
{
    public class SqlServerProviderTest : IAsyncLifetime
    {
        private static readonly IResourceProvider Sql =
            EmbeddedFileProvider<SqlServerProviderTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"sql\IOnymous\Config"));

        private readonly IResourceProvider _config;

        public SqlServerProviderTest()
        {
            _config = new SqlServerProvider("name=TestDb")
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
            };
        }

        public async Task InitializeAsync()
        {
            var connectionString = ConnectionStringRepository.Default.GetConnectionString("name=TestDb");
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(Sql.ReadTextFile("seed-test-data.sql"));
            }
        }

        [Fact]
        public async Task Can_deserialize_various_types()
        {
            var building = From<IBuildingConfig>.This;

            Assert.Equal("Tower Bridge", await _config.ReadSettingAsync(building.Select(x => x.Description)));
            Assert.Equal(true, await _config.ReadSettingAsync(building.Select(x => x.IsMonument)));
            Assert.Equal(65, await _config.ReadSettingAsync(building.Select(x => x.Height)));
            Assert.Equal(2.25, await _config.ReadSettingAsync(building.Select(x => x.AverageVisitorCount)));
            Assert.Equal(new DateTime(1894, 6, 30), await _config.ReadSettingAsync(building.Select(x => x.OpenedOn)));
            Assert.Equal(new[] { 11, 12 }, await _config.ReadSettingAsync(building.Select(x => x.Showtimes)));
            Assert.Equal(TimeSpan.Parse("01:15:00"), await _config.ReadSettingAsync(building.Select(x => x.AverageVisit)));
        }
        
        [Fact]
        public async Task Can_get_fallback_item()
        {
            var building = From<IBuildingConfig>.This;

            Assert.Equal("200kmh", await _config.ReadSettingAsync(From<ICarConfig>.Select(x => x.Speed)));
        }

        [Fact]
        public async Task Can_save_setting()
        {
            Assert.Equal("Tower Bridge", await _config.ReadSettingAsync(From<IBuildingConfig>.Select(x => x.Description)));

            await _config.WriteSettingAsync(From<IBuildingConfig>.Select(x => x.Description), "Tower Bridge new");

            Assert.Equal("Tower Bridge new", await _config.ReadSettingAsync(From<IBuildingConfig>.Select(x => x.Description)));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }

    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Config")]
    [SettingSelectorFormatter]
    public interface IBuildingConfig
    {
        string Description { get; }
        bool IsMonument { get; }
        int Height { get; }
        double AverageVisitorCount { get; }
        decimal Cost { get; }
        DateTime OpenedOn { get; }
        TimeSpan AverageVisit { get; }
        List<int> Showtimes { get; }
    }
    
    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Config")]
    [SettingSelectorFormatter]
    public interface ICarConfig
    {
        string Speed { get; }
    }
}