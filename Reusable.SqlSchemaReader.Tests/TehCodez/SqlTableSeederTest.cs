using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Utilities.SqlClient.SqlSchemas;
using System.Linq;

namespace Reusable.Utilities.SqlClient.Tests
{
    [TestClass]
    public class SqlTableSeederTest
    {
        private static readonly string ConnectonString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
        private const string Schema = "dbo";
        private const string Table = "SqlTableSeederTest";        

        [TestMethod]
        public void SeedAsync_WithoutId_Seeded()
        {
            var csvReader = CsvReader.FromFile(@"testdata\SqlTableSeederTest-without-id.csv");
            var sqlColumns = SqlHelper.Execute(ConnectonString, connection => connection.GetColumnFrameworkTypes(Schema, Table)).AsEnumerable();
            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, SqlTypeConverter.Default);

            SqlHelper.Execute(ConnectonString, connection => connection.Seed(Schema, Table, csv));

            Assert.AreEqual(3, SqlHelper.Execute(ConnectonString, connection =>
            {
                return connection.ExecuteQuery($"select count(*) from [{Schema}].[{Table}]", command => command.ExecuteScalar());                
            }));
        }

        [TestMethod]
        public void SeedAsync_WithId_Seeded()
        {
            var csvReader = CsvReader.FromFile(@"testdata\SqlTableSeederTest-with-id.csv");

            var sqlColumns = SqlHelper.Execute(ConnectonString, connection => connection.GetColumnFrameworkTypes(Schema, Table));

            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, SqlTypeConverter.Default);
            SqlHelper.Execute(ConnectonString, connection => connection.Seed(Schema, Table, csv));
            Assert.AreEqual(2, SqlHelper.Execute(ConnectonString, connection =>
           {
               return connection.ExecuteQuery($"select count(*) from [{Schema}].[{Table}]", command => command.ExecuteScalar());
           }));
        }
    }
}
