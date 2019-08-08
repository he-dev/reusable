using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Reusable.Data.Repositories;
using Reusable.Extensions;
using Reusable.Quickey;
using Xunit;

namespace Reusable.IOnymous.Config.Providers
{
    public class SqlServerProviderTest : IAsyncLifetime
    {
        private static readonly string ConnectionString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        private static readonly IResourceProvider Sql = new EmbeddedFileProvider(typeof(SqlServerProviderTest).Assembly, "Reusable").DecorateWith(RelativeProvider.Factory(@"sql\IOnymous\Config"));

        private readonly IResourceProvider _configuration;

        public SqlServerProviderTest()
        {
            _configuration = new SqlServerProvider(ConnectionString)
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
            var connectionString = ConnectionStringRepository.Default.GetConnectionString(ConnectionString);
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(Sql.ReadTextFile("seed-test-data.sql"));
            }
        }

        [Fact]
        public void Can_deserialize_various_types()
        {
            var testDto = new TestDto(_configuration);

            Assert.Equal("Tower Bridge", testDto.String);
            Assert.Equal(true, testDto.Boolean);
            Assert.Equal(65, testDto.Int32);
            Assert.Equal(2.25, testDto.Double);
            Assert.Equal(10.3m, testDto.Decimal);
            Assert.Equal(new DateTime(1894, 6, 30), testDto.DateTime);
            Assert.Equal(TimeSpan.Parse("01:15:00"), testDto.TimeSpan);
            Assert.Equal(new[] { 11, 12 }, testDto.ListOfInt32); // fallback
        }

        [Fact]
        public void Can_update_setting()
        {
            var testDto = new TestDto(_configuration);
            Assert.Equal("Tower Bridge", testDto.String);
            testDto.String = "Tower Bridge new";
            Assert.Equal("Tower Bridge new", testDto.String);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }


    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class TestDto
    {
        private readonly IResourceProvider _configuration;
        
        public TestDto(IResourceProvider configuration) => _configuration = configuration;

        public string String
        {
            get => _configuration.ReadSetting(() => String);
            set => _configuration.WriteSetting(() => String, value);
        }

        public bool Boolean => _configuration.ReadSetting(() => Boolean);
        public int Int32 => _configuration.ReadSetting(() => Int32);
        public double Double => _configuration.ReadSetting(() => Double);
        public decimal Decimal => _configuration.ReadSetting(() => Decimal);
        public DateTime DateTime => _configuration.ReadSetting(() => DateTime);
        public TimeSpan TimeSpan => _configuration.ReadSetting(() => TimeSpan);
        public List<int> ListOfInt32 => _configuration.ReadSetting(() => ListOfInt32);
    }
}