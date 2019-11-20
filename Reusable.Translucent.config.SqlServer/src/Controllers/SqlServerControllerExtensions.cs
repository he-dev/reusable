using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class SqlServerControllerExtensions
    {
        public static IResourceCollection AddSqlServer(this IResourceCollection controllers, string connectionString, IImmutableContainer? properties = default)
        {
            return controllers.Add(new SqlServerController(connectionString, properties));
        }
    }
}