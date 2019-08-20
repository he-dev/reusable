using System;
using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class SqlServerControllerExtensions
    {
        public static IResourceControllerBuilder AddSqlServer(this IResourceControllerBuilder builder, string connectionString, Action<SqlServerController> configure)
        {
            var sqlServer = new SqlServerController(connectionString);
            configure?.Invoke(sqlServer);
            return builder.AddController(sqlServer);
        }
        
        public static IResourceControllerBuilder AddSqlServer(this IResourceControllerBuilder builder, IImmutableContainer properties, Action<SqlServerController> configure)
        {
            var sqlServer = new SqlServerController(properties);
            configure?.Invoke(sqlServer);
            return builder.AddController(sqlServer);
        }
    }
}