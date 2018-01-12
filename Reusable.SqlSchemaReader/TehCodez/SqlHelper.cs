using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Utilities.SqlClient
{
    [PublicAPI]
    public static class SqlHelper
    {
        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(string connectionString, Func<SqlConnection, CancellationToken, Task<T>> body, CancellationToken cancellationToken)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return (await body(connection, cancellationToken)).Next(_ => scope.Complete());
            }
        }

        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static async Task ExecuteAsync(string connectionString, Func<SqlConnection, CancellationToken, Task> body, CancellationToken cancellationToken)
        {
            //return ExecuteAsync(connectionString, connection =>
            //{
            //    body(connection);
            //    return Task.FromResult<object>(null);
            //});

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await body(connection, cancellationToken);
                scope.Complete();
            }
        }

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
        public static void Execute(string connectionString, Action<SqlConnection> execute)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                execute(connection);
                scope.Complete();
            }
        }        
    }
}
