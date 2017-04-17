using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Tests;
using Reusable.Data;

namespace Reusable.ConfigWhiz.Datastores.Tests
{
    [TestClass]
    public class ConfigurationTestSqlServer : ConfigurationTestDatastore
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var ns = typeof(Foo).Namespace;

            Datastores = new IDatastore[]
            {
                new Reusable.ConfigWhiz.Datastores.SqlServer("name=TestDb"), 
            };

            Utils.ResetData();
        }

        private static class Utils
        {
            public static void ResetData()
            {
                using (var sqlConnection = OpenSqlConnection())
                using (var sqlCommand = sqlConnection.CreateCommand())
                using (var transaction = sqlConnection.BeginTransaction())
                {
                    sqlCommand.Transaction = transaction;
                    try
                    {
                        sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Resources.CreateSettingTables.sql");
                        sqlCommand.ExecuteNonQuery();

                        sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Resources.TruncateSettingTables.sql");
                        sqlCommand.ExecuteNonQuery();

                        // Insert test data.
                        InsertSetting1(sqlCommand);
                        //InsertSetting3(sqlCommand);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            private static void InsertSetting1(SqlCommand sqlCommand)
            {
                sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTestSqlServer>("Resources.InsertSetting1.sql");
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 200);
                sqlCommand.Parameters.Add("@Value", SqlDbType.NVarChar, -1);

                foreach (var testSetting in SettingFactory.ReadSettings<ConfigurationTestSqlServer>())
                {
                    sqlCommand.Parameters["@Name"].Value = testSetting.Path.ToFullStrongString();
                    sqlCommand.Parameters["@Value"].Value = testSetting.Value;
                    sqlCommand.ExecuteNonQuery();
                }
            }

            //private static void InsertSetting3(SqlCommand sqlCommand)
            //{
            //    sqlCommand.CommandText = ResourceReader.ReadEmbeddedResource<ConfigurationTest_SqlServer>("Resources.InsertSetting3.sql");
            //    sqlCommand.Parameters.Clear();
            //    sqlCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50);
            //    sqlCommand.Parameters.Add("@Value", SqlDbType.NVarChar, -1);
            //    sqlCommand.Parameters.Add("@Environment", SqlDbType.NVarChar, 50);
            //    sqlCommand.Parameters.Add("@Config", SqlDbType.NVarChar, 50);

            //    foreach (var testSetting in TestSettingFactory.CreateTestSettings3())
            //    {
            //        sqlCommand.Parameters["@Name"].Value = testSetting.Name;
            //        sqlCommand.Parameters["@Value"].Value = testSetting.Value;
            //        sqlCommand.Parameters["@Environment"].Value = testSetting.Tags["Environment"];
            //        sqlCommand.Parameters["@Config"].Value = testSetting.Tags["Config"];
            //        sqlCommand.ExecuteNonQuery();
            //    }
            //}

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
