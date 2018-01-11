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
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionString"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionString"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
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
        /// <param name="connectionString"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
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
    }
}
