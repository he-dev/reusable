using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.FileFormats.Csv;
using Reusable.Utilities.SqlClient;
using Reusable.Utilities.SqlClient.SqlSchemas;

namespace Reusable.Utilities.MSTest.Tests.Utilities
{
    [TestClass]
    public class SqlTableSeederTest
    {
        private const string ConnectonString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";
        private const string Schema = "dbo";
        private const string Table = "SqlTableSeederTest";
        private static readonly ITypeConverter Converter =
            TypeConverter
                .Empty
                .Add<StringToInt32Converter>()
                .Add<StringToDateTimeConverter>();

        [TestMethod]
        public void SeedAsync_WithoutId_Seeded()
        {
            var csvReader = CsvReader.FromFile(@"testdata\SqlTableSeederTest-without-id.csv");
            var sqlColumns = SqlHelper.Execute(ConnectonString, connection => connection.GetColumnFrameworkTypes(Schema, Table));
            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, Converter);

            SqlHelper.Execute(ConnectonString, connection => connection.Seed((Schema, Table), csv));

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

            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, Converter);
            SqlHelper.Execute(ConnectonString, connection => connection.Seed((Schema, Table), csv));
            Assert.AreEqual(2, SqlHelper.Execute(ConnectonString, connection =>
           {
               return connection.ExecuteQuery($"select count(*) from [{Schema}].[{Table}]", command => command.ExecuteScalar());
           }));
        }
    }
}
