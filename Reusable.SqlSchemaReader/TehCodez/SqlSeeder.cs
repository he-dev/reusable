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
        public static async Task SeedAsync([NotNull] this SqlConnection connection, [NotNull] SqlFourPartName objectName, [NotNull] DataTable data, bool truncate = true)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (Transaction.Current == null)
            {
                throw new InvalidOperationException($"{nameof(SeedAsync)} can be executed only within a transaction scope.");
            }

            var identifier = objectName.Render(connection);

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

        public static void Seed(this SqlConnection connection, SqlFourPartName objectName, DataTable data, bool truncate = true)
        {
            SeedAsync(connection, objectName, data, truncate).GetAwaiter().GetResult();
        }
    }
}
