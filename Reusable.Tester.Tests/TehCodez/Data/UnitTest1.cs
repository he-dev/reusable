using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Data.SqlClient;
using Reusable.Tester.Data;

namespace Reusable.Tester.Tests.Data
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var connectionString = "Data Source=(local);Initial Catalog=SmartConfigTest;Integrated Security=SSPI;";
            
            var csvReaderFactory = new CsvReaderFactory();
            var csvReader = csvReaderFactory.CreateFromFile(@"testdata\seed-no-id.csv");

            var columnTypes = await SqlHelper.ExecuteAsync(connectionString, conn => conn.GetColumnFrameworkTypes("dbo", "Seed2"));
            var converter = TypeConverter.Empty;
            var csv = csvReader.AsEnumerable().ToDataTable(columnTypes, converter);
            var sqlTableSeeder = new SqlTableSeeder(connectionString, "dbo", "Seed2");
            await sqlTableSeeder.SeedAsync(csv);
        }
    }
}
