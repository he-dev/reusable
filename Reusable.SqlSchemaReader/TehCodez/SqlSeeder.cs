using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;

namespace Reusable.Utilities.SqlClient
{
    [PublicAPI]
    public static class SqlSeeder
    {
        public static async Task SeedAsync([NotNull] this SqlConnection connection, [NotNull] string schema, [NotNull] string table, [NotNull] DataTable data, bool truncate = true)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (Transaction.Current == null)
            {
                throw new InvalidOperationException($"{nameof(SeedAsync)} can be executed only within a transaction scope.");
            }

            var identifier = connection.CreateIdentifier(schema, table);

            if (truncate)
            {
                // Using "truncate" because some databases/tables do not allow "delete".
                await connection.ExecuteQueryAsync($"truncate table {identifier}", (command, ct) => command.ExecuteNonQueryAsync(ct), CancellationToken.None);
            }

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, null))
            {
                bulkCopy.DestinationTableName = identifier;

                // Mapping is necessary because the source data-table might have fewer columns or in different order then the target table.
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
