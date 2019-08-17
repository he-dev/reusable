using System;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class SqlServerControllerExtensions
    {
        public static IResourceControllerBuilder UseSqlServer(this IResourceControllerBuilder builder, string connectionString, Action<SqlServerController> configure)
        {
            var sqlServer = new SqlServerController(connectionString);
            configure?.Invoke(sqlServer);
            return builder.AddController(sqlServer);
        }
    }
}