﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Reusable.Essentials;

namespace Reusable.Utilities.SqlClient
{
    [PublicAPI]
    public static class SqlHelper
    {
        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(string nameOrConnectionString, Func<SqlConnection, CancellationToken, Task<T>> body, CancellationToken cancellationToken)
        {
            var connectionString = AppConfigHelper.GetConnectionString(nameOrConnectionString);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return await body(connection, cancellationToken).ContinueWith(t =>
            {
                scope.Complete();
                return t.Result;
            }, cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static async Task ExecuteAsync(string nameOrConnectionString, Func<SqlConnection, CancellationToken, Task> body, CancellationToken cancellationToken)
        {
            var connectionString = AppConfigHelper.GetConnectionString(nameOrConnectionString);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await body(connection, cancellationToken).ContinueWith(_ => scope.Complete(), cancellationToken);
        }

        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static T Execute<T>(string nameOrConnectionString, Func<SqlConnection, T> execute)
        {
            var connectionString = AppConfigHelper.GetConnectionString(nameOrConnectionString);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return execute(connection).Also(_ => scope.Complete());
        }

        /// <summary>
        /// Executes the specified action within a transaction scope.
        /// </summary>
        public static void Execute(string nameOrConnectionString, Action<SqlConnection> execute)
        {
            var connectionString = AppConfigHelper.GetConnectionString(nameOrConnectionString);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            execute(connection);
            scope.Complete();
        }

        public static Task<T> ExecuteQueryAsync<T>(this SqlConnection connection, string query, Func<SqlCommand, CancellationToken, Task<T>> body, CancellationToken cancellationToken)
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            return body(command, cancellationToken);
        }

        public static T ExecuteQuery<T>(this SqlConnection connection, string query, Func<SqlCommand, T> body)
        {
            return
                connection
                    .ExecuteQueryAsync(query, (command, _) => Task.FromResult(body(command)), CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
        }
    }
}