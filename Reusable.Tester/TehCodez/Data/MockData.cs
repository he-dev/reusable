using Reusable.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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

    public interface ISeeder
    {
        Task SeedAsync(DataTable data, SeedMode mode);
    }

    public class SqlTableSeeder : ISeeder
    {
        private readonly string _connectionString;
        private readonly string _schema;
        private readonly string _table;

        public SqlTableSeeder(string connectionString, string schema, string table)
        {
            _connectionString = connectionString;
            _schema = schema;
            _table = table;
        }

        public async Task SeedAsync(DataTable data, SeedMode mode = SeedMode.Replace)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(_connectionString).Then(async c => await c.OpenAsync()))
            //using (var transaction = connection.BeginTransaction())
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                var identifier = CreateIdentifier(connection);

                if (mode == SeedMode.Replace)
                {
                    // Using "truncate" because some databases/tables do not allow "delete".
                    command.CommandText = $"truncate table {identifier}";
                    await command.ExecuteNonQueryAsync();
                }

                var columns = data.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToSoftString());
                using (await ToggleIdentityInsertAsync(command, columns))
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = identifier;
                    await bulkCopy.WriteToServerAsync(data);
                }

                scope.Complete();
            }
        }

        private string CreateIdentifier(SqlConnection connection)
        {
            using (var commandBuilder = DbProviderFactories.GetFactory(connection).CreateCommandBuilder())
            {
                return $"{commandBuilder.QuoteIdentifier(_schema)}.{commandBuilder.QuoteIdentifier(_table)}";
            }
        }

        private async Task<IDisposable> ToggleIdentityInsertAsync(SqlCommand command, IEnumerable<SoftString> columns)
        {
            var identityColumns = command.Connection.GetIdentityColumns(_schema, _table).Select(x => x.Name.ToSoftString()).ToList();
            var containsIdentityColumns = columns.Any(column => identityColumns.Contains(column));

            if (containsIdentityColumns)
            {
                var identifier = CreateIdentifier(command.Connection);
                command.CommandText = $"set identity_insert {identifier} off";
                await command.ExecuteNonQueryAsync();

                return Disposable.Create(async () =>
                {
                    command.CommandText = $"set identity_insert {identifier} on";
                    await command.ExecuteNonQueryAsync();
                });
            }

            return Disposable.Create(() => { });
        }
    }
}
