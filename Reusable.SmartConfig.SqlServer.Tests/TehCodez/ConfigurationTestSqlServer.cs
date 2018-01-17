using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.Reflection;
using Reusable.SmartConfig.Tests;
using Reusable.SmartConfig.Tests.Common;

namespace Reusable.SmartConfig.Datastores.Tests
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
            ResetData(Environment, Version, Salt);

            Datastores = new IDatastore[]
            {
                new SqlServer("name=TestDb")
                {
                    Schema = "dbo",
                    Table = "Setting3",
                    Where = new Dictionary<string, object>
                    {
                        ["Environment"] = "Test",
                        ["Version"] = "1.0"
                    }
                }
            };
        }

        public static void ResetData(string environment, string version, string salt)
        {
            //using (var trans = new TransactionScope())
            using (var conn = OpenSqlConnection())
            using (var cmd = conn.CreateCommand())
            using (var trans = conn.BeginTransaction())
            {
                cmd.Transaction = trans;
                try
                {
                    TruncateSetting3(cmd);
                    InsertSetting3(cmd, environment, version);
                    InsertSetting3(cmd, environment + salt, version + salt);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        private static void TruncateSetting3(IDbCommand cmd)
        {
            cmd.CommandText = ResourceReader.Default.FindString(name => name.Contains(nameof(TruncateSetting3)));
            cmd.ExecuteNonQuery();
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

        private static void InsertSetting3(SqlCommand cmd, string environment, string version)
        {
            cmd.CommandText = ResourceReader.Default.FindString(name => name.Contains(nameof(InsertSetting3)));
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200);
            cmd.Parameters.Add("@Value", SqlDbType.NVarChar, -1);
            cmd.Parameters.Add("@Environment", SqlDbType.NVarChar, 50);
            cmd.Parameters.Add("@Version", SqlDbType.NVarChar, 50);

            foreach (var setting in TestSettingRepository.Settings)
            {
                cmd.Parameters["@Name"].Value = setting.Name.ToString();
                cmd.Parameters["@Value"].Value = setting.Value;
                cmd.Parameters["@Environment"].Value = environment;
                cmd.Parameters["@Version"].Value = version;
                cmd.ExecuteNonQuery();
            }
        }

        private static SqlConnection OpenSqlConnection()
        {
            var connectionString = new ConnectionStringRepository().GetConnectionString("name=TestDb");
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}
