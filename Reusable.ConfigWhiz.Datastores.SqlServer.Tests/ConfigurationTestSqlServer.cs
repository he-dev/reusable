using System;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Paths;
using Reusable.ConfigWhiz.Tests;
using Reusable.ConfigWhiz.Tests.Common;
using Reusable.Data;

namespace Reusable.ConfigWhiz.Datastores.Tests
{
    [TestClass]
    public class ConfigurationTestSqlServer : ConfigurationTestBase
    {
        private const string Environment = "Test";
        private const string Version = "1.0";
        private const string Salt = "-fake";

        [TestInitialize]
        public void TestInitialize()
        {
            var ns = typeof(TestConsumer).Namespace;

            Datastores = new IDatastore[]
            {
                new SqlServer(
                    "SqlServer1",
                    "name=TestDb",
                    TableMetadata<SqlDbType>
                        .Create("dbo", "Setting3")
                            .AddNameColumn()
                            .AddValueColumn()
                            .AddColumn("Environment", SqlDbType.NVarChar, 200)
                            .AddColumn("Version", SqlDbType.NVarChar, 50),
                    ImmutableDictionary<string, object>.Empty.Add("Environment", "Test").Add("Version", "1.0")
                )
            };

            Utils.ResetData(Environment, Version, Salt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ctor_MissingDefaultColumns_Throws()
        {
            var store = new SqlServer(
                "SqlServer1",
                "name=TestDb",
                TableMetadata<SqlDbType>
                    .Create("dbo", "Setting3")
                    .AddColumn("Environment", SqlDbType.NVarChar, 200)
                    .AddColumn("Version", SqlDbType.NVarChar, 50),
                ImmutableDictionary<string, object>.Empty.Add("Environment", "Test").Add("Version", "1.0")
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ctor_MissingConstraints_Throws()
        {
            var store = new SqlServer(
                "SqlServer1",
                "name=TestDb",
                TableMetadata<SqlDbType>
                    .Create("dbo", "Setting3")
                    .AddNameColumn()
                    .AddValueColumn()
                    .AddColumn("Environment", SqlDbType.NVarChar, 200)
                    .AddColumn("Version", SqlDbType.NVarChar, 50),
                ImmutableDictionary<string, object>.Empty.Add("Environment", "Test")
            );
        }

        private static class Utils
        {
            public static void ResetData(string environment, string version, string salt)
            {
                using (var sqlConnection = OpenSqlConnection())
                using (var sqlCommand = sqlConnection.CreateCommand())
                using (var transaction = sqlConnection.BeginTransaction())
                {
                    sqlCommand.Transaction = transaction;
                    try
                    {
                        sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Resources.Truncate Setting tables.sql");
                        sqlCommand.ExecuteNonQuery();

                        //sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Insert Setting3.sql");
                        //sqlCommand.ExecuteNonQuery();

                        // Insert test data.
                        //InsertSetting1(sqlCommand);
                        InsertSetting3(sqlCommand, environment, version);
                        InsertSetting3(sqlCommand, environment + salt, version + salt);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            //private static void InsertSetting1(SqlCommand sqlCommand)
            //{
            //    sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Resources.Insert Setting1.sql");
            //    sqlCommand.Parameters.Clear();
            //    sqlCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 200);
            //    sqlCommand.Parameters.Add("@Value", SqlDbType.NVarChar, -1);

            //    foreach (var testSetting in SettingFactory.ReadSettings<ConfigurationTestSqlServer>())
            //    {
            //        sqlCommand.Parameters["@Name"].Value = testSetting.Path.ToFullStrongString();
            //        sqlCommand.Parameters["@Value"].Value = testSetting.Value;
            //        sqlCommand.ExecuteNonQuery();
            //    }
            //}

            private static void InsertSetting3(SqlCommand sqlCommand, string environment, string version)
            {
                sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Resources.Insert Setting3.sql");
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 200);
                sqlCommand.Parameters.Add("@Value", SqlDbType.NVarChar, -1);
                sqlCommand.Parameters.Add("@Environment", SqlDbType.NVarChar, 50);
                sqlCommand.Parameters.Add("@Version", SqlDbType.NVarChar, 50);

                foreach (var setting in SettingFactory.ReadSettings<TestConsumer>())
                {
                    sqlCommand.Parameters["@Name"].Value = setting.Id.ToFullStrongString();
                    sqlCommand.Parameters["@Value"].Value = setting.Value;
                    sqlCommand.Parameters["@Environment"].Value = environment;
                    sqlCommand.Parameters["@Version"].Value = version;
                    sqlCommand.ExecuteNonQuery();
                }
            }

            private static SqlConnection OpenSqlConnection()
            {
                var connectionString = new AppConfigRepository().GetConnectionString("TestDb");
                var sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                return sqlConnection;
            }
        }
    }
}
