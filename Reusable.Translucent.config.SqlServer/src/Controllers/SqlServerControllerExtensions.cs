using System;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class SqlServerControllerExtensions
    {
        public static IResourceCollection AddSqlServer(this IResourceCollection controllers, string? id,  string connectionString, Action<SqlServerController>? configure = default)
        {
            return controllers.Add(new SqlServerController(id, connectionString).Pipe(configure));
        }
    }
}