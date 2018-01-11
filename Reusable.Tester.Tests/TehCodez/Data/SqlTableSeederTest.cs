using System;
using System.Data;
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
        public async Task SeedAsync_WithoutId_Seeded()
        {
            var csvReader = CsvReader.FromFile(@"testdata\SqlTableSeederTest-without-id.csv");
            var sqlColumns = SqlHelper.Execute(ConnectonString, conn => conn.GetColumnFrameworkTypes(Schema, Table));
            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, Converter);

            await SqlHelper.ExecuteAsync(ConnectonString, conn => conn.SeedAsync(Schema, Table, csv));
            Assert.AreEqual(3, await SqlHelper.ExecuteAsync(ConnectonString, async connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"select count(*) from [{Schema}].[{Table}]";
                    return await command.ExecuteScalarAsync();
                }
            }));
        }

        [TestMethod]
        public async Task SeedAsync_WithId_Seeded()
        {

            var csvReader = CsvReader.FromFile(@"testdata\SqlTableSeederTest-with-id.csv");

            var sqlColumns =
                SqlHelper
                    .Execute(ConnectonString, conn => conn.GetColumnSchemas(new SqlColumnSchema { TableSchema = Schema, TableName = Table }))
                    .Select(x => (Name: x.ColumnName, Type: x.FrameworkType))
                    .ToList();
            var csv = csvReader.AsEnumerable().ToDataTable(sqlColumns, Converter);
            await SqlHelper.ExecuteAsync(ConnectonString, conn => conn.SeedAsync(Schema, Table, csv));
            Assert.AreEqual(2, await SqlHelper.ExecuteAsync(ConnectonString, async connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"select count(*) from [{Schema}].[{Table}]";
                    return await command.ExecuteScalarAsync();
                }
            }));
        }
    }
}
