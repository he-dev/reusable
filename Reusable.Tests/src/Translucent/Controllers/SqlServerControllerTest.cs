using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.Exceptionize;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class SqlServerControllerTest : IAsyncLifetime, IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        public SqlServerControllerTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }
        
        public async Task InitializeAsync()
        {
            var connectionString = AppConfigHelper.GetConnectionString(TestHelper.ConnectionString);
            await using var conn = new SqlConnection(connectionString);
            await conn.ExecuteAsync(_testHelper.Resources.ReadTextFile(@"Translucent/Config/seed-test-data.sql"));
        }

        [Fact]
        public void Can_deserialize_various_types()
        {
            var testDto = new TestDto(_testHelper.Resources);

            Assert.Equal("Tower Bridge", testDto.String);
            Assert.Equal(true, testDto.Boolean);
            Assert.Equal(65, testDto.Int32);
            Assert.Equal(2.25, testDto.Double);
            Assert.Equal(10.3m, testDto.Decimal);
            Assert.Equal(new DateTime(1894, 6, 30), testDto.DateTime);
            Assert.Equal(TimeSpan.Parse("01:15:00"), testDto.TimeSpan);
            var listOfInt32a = testDto.ListOfInt32; // fallback
            var listOfInt32b = testDto.ListOfInt32; // fallback
            Assert.Equal(new[] { 11, 12 }, listOfInt32a); // fallback
            Assert.Same(listOfInt32a, listOfInt32b);

            var validationException = Assert.ThrowsAny<DynamicException>(() => testDto.Color);
        }

        [Fact]
        public void Can_update_setting()
        {
            var testDto = new TestDto(_testHelper.Resources);
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
        private readonly IResource _resources;

        public TestDto(IResource resources) => _resources = resources;

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
        public List<int> ListOfInt32 => _resources.ReadSetting(() => ListOfInt32, maxAge: TimeSpan.FromSeconds(7));

        [RegularExpression("Red|Blue")]
        public string Color => _resources.ReadSetting(() => Color);
    }
}