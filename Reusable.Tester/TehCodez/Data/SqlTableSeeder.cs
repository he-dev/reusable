using Reusable.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Reusable.Collections;
using Reusable.Converters;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.Tester.Data
{
    public enum SeedMode
    {
        Append,
        Replace
    }

    public static class SqlTableSeeder
    {
        public static async Task SeedAsync(this SqlConnection connection, string schema, string table, DataTable data, SeedMode mode = SeedMode.Replace)
        {
            if (Transaction.Current == null) { throw new InvalidOperationException($"{nameof(SeedAsync)} can be executed only within a transaction scope."); }

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                var identifier = connection.CreateIdentifier(schema, table);

                if (mode == SeedMode.Replace)
                {
                    // Using "truncate" because some databases/tables do not allow "delete".
                    command.CommandText = $"truncate table {identifier}";
                    await command.ExecuteNonQueryAsync();
                }

                var columns = data.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToSoftString());
                using (await connection.ToggleIdentityInsertAsync(schema, table, columns))
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = identifier;
                    await bulkCopy.WriteToServerAsync(data);
                }
            }
        }

        private static async Task<IDisposable> ToggleIdentityInsertAsync(this SqlConnection connection, string schema, string table, IEnumerable<SoftString> columns)
        {
            var identityColumns = connection.GetIdentityColumns(schema, table).Select(x => x.Name.ToSoftString()).ToList();
            var containsIdentityColumns = columns.Any(column => identityColumns.Contains(column));

            if (containsIdentityColumns)
            {
                var identifier = connection.CreateIdentifier(schema, table);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"set identity_insert {identifier} off";
                    await command.ExecuteNonQueryAsync();
                }

                return Disposable.Create(async () =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"set identity_insert {identifier} on";
                        await command.ExecuteNonQueryAsync();
                    }
                });
            }

            return Disposable.Create(() => { });
        }
    }
}
