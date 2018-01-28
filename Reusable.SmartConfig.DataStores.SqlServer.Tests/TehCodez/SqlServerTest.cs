﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.Exceptionize;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;
using Reusable.Tester;
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
                    .AddRow("qux", "barz", "baaaz")
                    .AddRow("qux", "barx", "quuux");

            SqlHelper.Execute("name=TestDb", connection =>
            {
                connection.Seed(Schema, Table, data);
            });
        }

        [TestMethod]
        public void Read_ByName_Setting()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(
                    Arg.Matches<object>(value => value.Equals("fooo")),
                    Arg.Matches<Type>(type => type == typeof(string))
                ))
                .Returns(obj => obj)
                .Occurs(1);

            var sqlServer = new SqlServer("name=TestDb", converter)
            {
                SettingTableName = (Schema, Table),
                ColumnMapping = ("_name", "_value")
            };

            var setting = sqlServer.Read("foo", typeof(string));

            converter.Assert();

            Assert.IsNotNull(setting);
            Assert.AreEqual("fooo", setting.Value);
        }

        [TestMethod]
        public void Read_ByNameAndOther_Setting()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(
                    Arg.Matches<object>(value => value.Equals("barx")),
                    Arg.Matches<Type>(type => type == typeof(string))
                ))
                .Returns(obj => obj)
                .Occurs(1);

            var sqlServer = new SqlServer("name=TestDb", converter)
            {
                SettingTableName = (Schema, Table),
                ColumnMapping = ("_name", "_value"),
                Where = new Dictionary<string, object>
                {
                    ["_other"] = "quuux"
                }
            };

            var setting = sqlServer.Read("qux", typeof(string));

            converter.Assert();

            Assert.IsNotNull(setting);
            Assert.AreEqual("barx", setting.Value);
        }

        [TestMethod]
        public void Read_ByAmbigousName_Throws()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(
                    Arg.IsAny<object>(),
                    Arg.IsAny<Type>()
                ))
                .OccursNever();

            var sqlServer = new SqlServer("name=TestDb", converter)
            {
                SettingTableName = (Schema, Table),
                ColumnMapping = ("_name", "_value")
            };

            var exception = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => sqlServer.Read("qux", typeof(string)));

            converter.Assert();
            Assert.AreEqual("SettingReadException", exception.GetType().Name);
            Assert.AreEqual("AmbiguousSettingException", exception.InnerException.GetType().Name);
        }

        [TestMethod]
        public void Write_ByName_ValueUpdated()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(
                    Arg.Matches<object>(value => value.Equals("fooo-updated")),
                    Arg.Matches<Type>(type => type == typeof(string))
                ))
                .Returns(obj => obj)
                .Occurs(1);

            Mock
                .Arrange(() => converter.Serialize(
                    Arg.Matches<object>(value => value.Equals("fooo-updated"))
                ))
                .Returns(obj => obj)
                .Occurs(1);


            var sqlServer = new SqlServer("name=TestDb", converter)
            {
                SettingTableName = (Schema, Table),
                ColumnMapping = ("_name", "_value")
            };

            sqlServer.Write(new Setting("foo") { Value = "fooo-updated" });
            var setting = sqlServer.Read("foo", typeof(string));

            converter.Assert();

            Assert.IsNotNull(setting);
            Assert.AreEqual("fooo-updated", setting.Value);
        }

        [TestMethod]
        public void Write_ByName_ValueInserted()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(
                    Arg.Matches<object>(value => value.Equals("fooo-inserted")),
                    Arg.Matches<Type>(type => type == typeof(string))
                ))
                .Returns(obj => obj)
                .Occurs(1);

            Mock
                .Arrange(() => converter.Serialize(
                    Arg.Matches<object>(value => value.Equals("fooo-inserted"))
                ))
                .Returns(obj => obj)
                .Occurs(1);


            var sqlServer = new SqlServer("name=TestDb", converter)
            {
                SettingTableName = (Schema, Table),
                ColumnMapping = ("_name", "_value")
            };

            sqlServer.Write(new Setting("foo_new") { Value = "fooo-inserted" });
            var setting = sqlServer.Read("foo_new", typeof(string));

            converter.Assert();

            Assert.IsNotNull(setting);
            Assert.AreEqual("fooo-inserted", setting.Value);
        }
    }
}
