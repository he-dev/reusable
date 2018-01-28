using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.SmartConfig.DataStores;
using Reusable.SmartConfig.SettingConverters;
using Reusable.SmartConfig.Utilities;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig.Tests.Integration
{
    [TestClass]
    public class ConfigurationTest
    {
        private static readonly SqlFourPartName SettingTableName = ("reusable", "SmartConfig.DataStores.Tests.SqlServerTest");

        [TestInitialize]
        public void TestInitialize()
        {
            var data =
                new DataTable()
                    .AddColumn("_name", column => column.DataType = typeof(string))
                    .AddColumn("_value", column => column.DataType = typeof(string))
                    .AddColumn("_other", column => column.DataType = typeof(string))
                    .AddRow("TestClass1.Foo", "123", "integration")
                    .AddRow("TestClass2.Bar", "text", "integration")
                    .AddRow("TestClass3.Baz", "1.23", "integration")
                    // Used for exeption testing.
                    .AddRow("TestClass3.Qux", "quux", "integration")
                    .AddRow("Reusable.SmartConfig.Tests+TestClass3.Qux", "quux", "integration")
                ;

            SqlHelper.Execute("name=TestDb", connection =>
            {
                connection.Seed(SettingTableName, data);
            });
        }

        [TestMethod]
        public void GetValue_ByExpression_Value()
        {
            var settingConverter = new JsonSettingConverter(typeof(string));
            var configuration = new Configuration(new ISettingDataStore[]
            {
                new AppSettings(settingConverter),
                new SqlServer("name=TestDb", settingConverter)
                {
                    SettingTableName = SettingTableName,
                    ColumnMapping = ("_name", "_value"),
                    Where = new Dictionary<string, object>
                    {
                        ["_other"] = "integration"
                    }
                },
            });

            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();

            var foo = configuration.GetValue(() => testClass1.Foo);
            var bar = configuration.GetValue(() => testClass2.Bar);

            Assert.AreEqual(123, foo);
            Assert.AreEqual("text", bar);
        }
    }

    internal class TestClass1
    {
        public int Foo { get; set; }
    }

    internal class TestClass2
    {
        public string Bar { get; set; }
    }
}