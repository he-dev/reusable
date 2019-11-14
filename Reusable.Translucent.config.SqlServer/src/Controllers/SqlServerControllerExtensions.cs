using Reusable.Data;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class SqlServerControllerExtensions
    {
        public static IResourceControllerBuilder AddSqlServer(this IResourceControllerBuilder builder, string connectionString, IImmutableContainer properties = default)
        {
            return builder.AddController(new SqlServerController(connectionString, properties.ThisOrEmpty()));
        }
    }
}