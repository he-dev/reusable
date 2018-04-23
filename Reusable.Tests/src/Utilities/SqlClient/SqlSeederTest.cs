using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Utilities.SqlClient.SqlSchemas;
using System.Linq;
using Reusable.Csv;
using Reusable.Csv.Utilities;

namespace Reusable.Utilities.SqlClient.Tests
{
    [TestClass]
    public class SqlSeederTest
    {
        private static readonly string ConnectonString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
        private static  readonly string TestData = Path.Combine("TestData", "SqlSeederTest");
        private const string Schema = "reusable";
        private const string Table = "Utilities.SqlClient.Tests.SqlSeederTest";

        [TestMethod]
        public void SeedAsync_DoesNotContainId_Seeded()
        {
            var csvReader = CsvReader.FromFile(Path.Combine(TestData, nameof(SeedAsync_DoesNotContainId_Seeded) + ".csv"));
            var sqlColumns = SqlHelper.Execute(ConnectonString, connection => connection.GetColumnFrameworkTypes(Schema, Table)).AsEnumerable();
            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, new SqlTypeConverter());

            SqlHelper.Execute(ConnectonString, connection => connection.Seed((Schema, Table), csv));

            Assert.AreEqual(3, SqlHelper.Execute(ConnectonString, connection =>
            {
                return connection.ExecuteQuery($"select count(*) from [{Schema}].[{Table}]", command => command.ExecuteScalar());
            }));
        }

        [TestMethod]
        public void SeedAsync_ContainsId_Seeded()
        {
            var csvReader = CsvReader.FromFile(Path.Combine(TestData, nameof(SeedAsync_ContainsId_Seeded) + ".csv"));
            var sqlColumns = SqlHelper.Execute(ConnectonString, connection => connection.GetColumnFrameworkTypes(Schema, Table));
            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, new SqlTypeConverter());

            SqlHelper.Execute(ConnectonString, connection => connection.Seed((Schema, Table), csv));

            Assert.AreEqual(2, SqlHelper.Execute(ConnectonString, connection =>
            {
                return connection.ExecuteQuery($"select count(*) from [{Schema}].[{Table}]", command => command.ExecuteScalar());
            }));
        }
    }
}
