using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Reusable.Extensions;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Data.SqlClient
{
    public static class SqlHelper
    {
        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static T Execute<T>(string connectionString, Func<SqlConnection, T> execute)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var result = execute(connection);
                scope.Complete();
                return result;
            }
        }

        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(string connectionString, Func<SqlConnection, Task<T>> execute)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = await execute(connection);
                scope.Complete();
                return result;
            }
        }

        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static async Task ExecuteAsync(string connectionString, Func<SqlConnection, Task> execute)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                await execute(connection);
                scope.Complete();                
            }
        }

        [NotNull, ContractAnnotation("connection: null => halt")]
        public static string CreateIdentifier([NotNull] this SqlConnection connection, params string[] names)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            using (var commandBuilder = DbProviderFactories.GetFactory(connection).CreateCommandBuilder())
            {
                // ReSharper disable once PossibleNullReferenceException - commandBuilder is never null for SqlConnection.
                return names.Select(commandBuilder.QuoteIdentifier).Join(".");
            }
        }

        // it turned out that sql-bulk-copy can already do this
        //public static async Task<IDisposable> ToggleIdentityInsertAsync(this SqlConnection connection, string schema, string table, IEnumerable<SoftString> columns)
        //{
        //    var identityColumns = connection.GetIdentityColumns(schema, table).Select(x => x.Name.ToSoftString()).ToList();
        //    var containsIdentityColumns = columns.Any(column => identityColumns.Contains(column));

        //    if (containsIdentityColumns)
        //    {
        //        var identifier = connection.CreateIdentifier(schema, table);
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = $"set identity_insert {identifier} on";
        //            await command.ExecuteNonQueryAsync();
        //        }

        //        return Disposable.Create(async () =>
        //        {
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = $"set identity_insert {identifier} off";
        //                await command.ExecuteNonQueryAsync();
        //            }
        //        });
        //    }

        //    return Disposable.Create(() => { });
        //}
    }
}
