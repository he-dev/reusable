using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.Reflection;
using Reusable.Utilities.SqlClient;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.DataStores.Tests
{
    [TestClass]
    public class SqlServerTest
    {
        private const string Schema = "reusable";
        private const string Table = "SmartConfig.DataStores.Tests.SqlServerTest";

        [TestInitialize]
        public void TestInitialize()
        {
            var data =
                new DataTable()
                    .AddColumn("_name", column => column.DataType = typeof(string))
                    .AddColumn("_value", column => column.DataType = typeof(string))
                    .AddColumn("_other", column => column.DataType = typeof(string))
                    .AddRow("foo", "fooo", null)
                    .AddRow("bar", "baar", null)
                    .AddRow("baz", "baaz", "baaaz")
                    .AddRow("qux", "bar", "baaaz")
                    .AddRow("qux", "bar", "quuux");

            SqlHelper.Execute("name=TestDb", connection =>
            {
                connection.Seed(Schema, Table, data);
            });            
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(Arg.IsAny<object>(), Arg.IsAny<Type>()))
                .Returns(obj => obj)
                .Occurs(2);

            var sqlServer = new SqlServer("name=TestDb", converter)
            {
                SettingTableName = (Schema, Table),
                ColumnMapping = new SqlServerColumnMapping
                {
                    Name = "_name",
                    Value = "_value"
                }
            };

            var foo = sqlServer.Read("foo", typeof(string));

            //converter.Assert();

            Assert.AreEqual("fooo", foo.Value);
        }        
    }
}
