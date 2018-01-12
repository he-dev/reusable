using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.SqlClient;

namespace Reusable.Data.SqlClient.Tests
{
    [TestClass]
    public class SqlSchemaReaderTest
    {
        private readonly ISqlSchema _sqlSchema = new SqlClient.SqlSchema();

        [TestMethod]
        public void GetSqlTableSchemas_TestingDb_OneTable()
        {
            var tableSchemas = _sqlSchema.GetSqlTableSchemas(
                connectionString: ConfigurationManager.ConnectionStrings["TestingDb"].ConnectionString,
                schemaRestriction: new SqlTableSchema
                {
                    TableCatalog = "TestingDb",
                    TableSchema = "reusable_data_sqlclient"
                });

            Assert.AreEqual(1, tableSchemas.Count);

            var sqlTableSchema = tableSchemas.Single();

            Assert.AreEqual("SqlSchemaReaderTest", sqlTableSchema.TableName);
        }

        [TestMethod]
        public void GetSqlColumnSchemas_TestingDb_ThreeColumns()
        {
            var columnSchemas = _sqlSchema.GetSqlColumnSchemas(
                connectionString: ConfigurationManager.ConnectionStrings["TestingDb"].ConnectionString,
                schemaRestriction: new SqlColumnSchema
                {
                    TableCatalog = "TestingDb",
                    TableSchema = "reusable_data_sqlclient"
                });

            Assert.AreEqual(3, columnSchemas.Count);

            //var sqlTableSchema = columnSchemas.Single();

            //Assert.AreEqual("SqlSchemaReaderTest", sqlTableSchema.TableName);
        }
    }
}
