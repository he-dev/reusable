using System;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidBuilderExtensions
    {
        public static ResourceSquidBuilder UseSqlServer(this ResourceSquidBuilder builder, string connectionString, Action<SqlServerController> configure)
        {
            var sqlServer = new SqlServerController(connectionString);
            configure?.Invoke(sqlServer);
            return builder.UseController(sqlServer);
        }
    }
}