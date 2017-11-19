using System.Collections.Immutable;
using System.Data;
using System.Data.SQLite;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Data;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Tests;
using Reusable.SmartConfig.Tests.Common;

// ReSharper disable InconsistentNaming

namespace Reusable.SmartConfig.Datastores.Tests
{
    [TestClass]
    public class ConfiguationTestSQLite : ConfigurationTestBase
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
                new SQLite(
                    "SQLite1",
                    "name=configdb",
                    TableMetadata<DbType>
                        .Create("dbo", "Setting3")
                        .AddColumn(EntityProperty.Name, DbType.String, 200)
                        .AddColumn(EntityProperty.Value, DbType.String, -1)
                        .AddColumn("Environment", DbType.String, 200)
                        .AddColumn("Version", DbType.String, 50),
                    ImmutableDictionary<string, object>.Empty
                        .Add("Environment", "Test")
                        .Add("Version", "1.0")
                ), 
            };

            Utils.ResetData(Environment, Version, Salt);
        }

        private static class Utils
        {
            public static void ResetData(string environment, string version, string salt)
            {
                // Insert test data.
                var connectionString = new AppConfigRepository().GetConnectionString("configdb");

                SQLiteConnection OpenSQLiteConnection()
                {
                    var sqLiteConnection = new SQLiteConnection(connectionString);
                    sqLiteConnection.Open();
                    return sqLiteConnection;
                }

                using (var connection = OpenSQLiteConnection())
                using (var command = connection.CreateCommand())
                using (var transaction = connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        command.CommandText = ResourceReader.ReadEmbeddedResource<ConfiguationTestSQLite>("Resources.Create Setting tables.sql");
                        command.ExecuteNonQuery();

                        //command.CommandText = ResourceReader.ReadEmbeddedResource<ConfiguationTestSQLite>("Resources.Truncate Setting tables.sql");
                        //command.ExecuteNonQuery();

                        // Insert test data.
                        InsertSetting3(command, environment, version);
                        InsertSetting3(command, environment + salt, version + salt);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            private static void InsertSetting3(SQLiteCommand command, string environment, string version)
            {
                command.CommandText = ResourceReader.ReadEmbeddedResource<ConfiguationTestSQLite>("Resources.Insert Setting3.sql");
                command.Parameters.Clear();
                command.Parameters.Add("@Name", DbType.String, 200);
                command.Parameters.Add("@Value", DbType.String, -1);
                command.Parameters.Add("@Environment", DbType.String, 50);
                command.Parameters.Add("@Version", DbType.String, 50);

                foreach (var setting in SettingFactory.ReadSettings())
                {
                    command.Parameters["@Name"].Value = setting.Name.ToString();
                    command.Parameters["@Value"].Value = setting.Value.ToString().Recode(Encoding.UTF8, Encoding.Default);
                    command.Parameters["@Environment"].Value = environment;
                    command.Parameters["@Version"].Value = version;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
