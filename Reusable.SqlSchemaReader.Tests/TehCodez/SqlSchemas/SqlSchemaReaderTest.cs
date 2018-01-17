using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.SqlClient.SqlSchemas;

namespace Reusable.Utilities.SqlClient.Tests.SqlSchemas
{
    [TestClass]
    public class SqlSchemaReaderTest
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

        [TestMethod]
        public void GetSqlTableSchemas_TestingDb_OneTable()
        {
            var tableSchemas = SqlHelper.Execute(ConnectionString, connection => connection.GetTableSchemas(schemaRestriction: new SqlTableSchema
            {
                TableCatalog = "TestDb",
                TableSchema = "reusable"
            }));

            Assert.AreEqual(1, tableSchemas.Count);

            var sqlTableSchema = tableSchemas.Single();

            Assert.AreEqual("Utilities.SqlClient.Tests.SqlSchemaReaderTest", sqlTableSchema.TableName);
        }

        [TestMethod]
        public void GetSqlColumnSchemas_TestingDb_ThreeColumns()
        {
            var columnSchemas = SqlHelper.Execute(ConnectionString, connection => connection.GetColumnSchemas(schemaRestriction: new SqlColumnSchema
            {
                TableCatalog = "TestDb",
                TableSchema = "reusable",
                TableName = "Utilities.SqlClient.Tests.SqlSchemaReaderTest"
            }));

            Assert.AreEqual(3, columnSchemas.Count);

            //var sqlTableSchema = columnSchemas.Single();

            //Assert.AreEqual("SqlSchemaReaderTest", sqlTableSchema.TableName);
        }
    }
}
