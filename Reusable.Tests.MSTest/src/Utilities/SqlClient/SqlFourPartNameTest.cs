using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Utilities.SqlClient.Tests
{
    [TestClass]
    public class SqlFourPartNameTest
    {
        // foo.bar.baz.qux

        [TestMethod]
        public void ToString_SchemaOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName("foo", "bar", null, "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[foo].[bar]..[qux]", identifier);
        }

        [TestMethod]
        public void ToString_DatabaseOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName("foo", null, "baz", "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[foo]..[baz].[qux]", identifier);
        }

        [TestMethod]
        public void ToString_DatabaseAndSchemaOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName("foo", null, null, "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[foo]...[qux]", identifier);
        }

        [TestMethod]
        public void ToString_ServerOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName(null, "bar", "baz", "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[bar].[baz].[qux]", identifier);
        }

        [TestMethod]
        public void ToString_ServerAndSchemaOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName(null, "bar", null, "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[bar]..[qux]", identifier);
        }

        [TestMethod]
        public void ToString_ServerAndDatabaseOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName(null, null, "baz", "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[baz].[qux]", identifier);
        }

        [TestMethod]
        public void ToString_ServerAndDatabaseAndSchemaOmitted_Identifier()
        {
            var fourPartName = new SqlFourPartName(null, null, null, "qux");
            var identifier = SqlHelper.Execute("name=TestDb", fourPartName.Render);
            Assert.AreEqual("[qux]", identifier);
        }
    }
}
