using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Reusable.Extensions;

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
        public static async Task<T> ExecuteAsync<T>(string connectionString, Func<SqlConnection, T> execute)
        {
            //var tcs = new TaskCompletionSource<T>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = execute(connection);
                scope.Complete();
                //return Task.FromResult(result);
                //tcs.SetResult(result);
                return result;
            }

            //return tcs.Task;
        }
    }
}
