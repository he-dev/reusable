using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Utilities;
using Reusable.Utilities.SqlClient;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class ConfigurationTest2
    {
        private static readonly string Namespace = typeof(ConfigurationTest2).Namespace;

        private static readonly SqlFourPartName SettingTableName = ("reusable", "SmartConfig.DataStores.Tests.SqlServerTest");

        [TestInitialize]
        public void TestInitialize()
        {
            var data =
                new DataTable()
                    .AddColumn("_name", typeof(string))
                    .AddColumn("_value", typeof(string))
                    .AddColumn("_other", typeof(string))
                    .AddRow("MockType1.Foo", "123", "integration")
                    .AddRow("MockType2.Bar", "text", "integration")
                    .AddRow("TestClass3.Baz", "1.23", "integration")
                    // Used for exeption testing.
                    .AddRow("TestClass3.Qux", "quux", "integration")
                    .AddRow($"{Namespace}+TestClass3.Qux", "quux", "integration")
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
            var configuration = new Configuration(new ISettingProvider[]
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