using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Reusable.Utilities.SqlClient;

namespace Reusable.Tester.Utilities
{
    [PublicAPI]
    public static class SqlTableSeeder
    {
        public static async Task SeedAsync(this SqlConnection connection, string schema, string table, DataTable data, bool truncate = true)
        {
            if (Transaction.Current == null)
            {
                throw new InvalidOperationException($"{nameof(SeedAsync)} can be executed only within a transaction scope.");
            }

            var identifier = connection.CreateIdentifier(schema, table);

            if (truncate)
            {
                // Using "truncate" because some databases/tables do not allow "delete".
                await connection.ExecuteQueryAsync($"truncate table {identifier}", command => command.ExecuteNonQueryAsync());
            }

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, null))
            {
                bulkCopy.DestinationTableName = identifier;

                foreach (var column in data.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
                {
                    bulkCopy.ColumnMappings.Add(column, column);
                }

                await bulkCopy.WriteToServerAsync(data);
            }
        }

        public static void Seed(this SqlConnection connection, string schema, string table, DataTable data, bool truncate = true)
        {
            SeedAsync(connection, schema, table, data, truncate).GetAwaiter().GetResult();
        }
    }
}
