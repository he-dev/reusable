using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Data.SqlClient.Tests
{
    [TestClass]
    public class SqlSchemaReaderTest
    {
        private readonly ISqlSchemaReader _sqlSchemaReader = new SqlClient.SqlSchemaReader();

        [TestMethod]
        public void GetSqlTableSchemas_TestingDb_OneTable()
        {
            var tableSchemas = _sqlSchemaReader.GetSqlTableSchemas(
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
            var columnSchemas = _sqlSchemaReader.GetSqlColumnSchemas(
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
