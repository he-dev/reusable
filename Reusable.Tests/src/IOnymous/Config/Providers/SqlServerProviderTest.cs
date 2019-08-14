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
        public async Task InitializeAsync()
        {
            var connectionString = ConnectionStringRepository.Default.GetConnectionString(TestHelper.ConnectionString);
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.ExecuteAsync(TestHelper.Resources.ReadTextFile(@"IOnymous/Config/seed-test-data.sql"));
            }
        }

        [Fact]
        public void Can_deserialize_various_types()
        {
            var testDto = new TestDto(TestHelper.Resources);

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
            var testDto = new TestDto(TestHelper.Resources);
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
        private readonly IResourceSquid _resources;

        public TestDto(IResourceSquid resources) => _resources = resources;

        public string String
        {
            get => _resources.ReadSetting(() => String);
            set => _resources.WriteSetting(() => String, value);
        }

        public bool Boolean => _resources.ReadSetting(() => Boolean);
        public int Int32 => _resources.ReadSetting(() => Int32);
        public double Double => _resources.ReadSetting(() => Double);
        public decimal Decimal => _resources.ReadSetting(() => Decimal);
        public DateTime DateTime => _resources.ReadSetting(() => DateTime);
        public TimeSpan TimeSpan => _resources.ReadSetting(() => TimeSpan);
        public List<int> ListOfInt32 => _resources.ReadSetting(() => ListOfInt32);
    }
}