using System;

namespace Reusable.SmartConfig.Datastores
{
    public static class SqlServerConfigurationOptionsExtensions
    {
        public static IConfigurationProperties UseSqlServer(this IConfigurationProperties configuration, string nameOrConnectionString)
        {
            configuration.Datastores.Add(new SqlServer(nameOrConnectionString));
            return configuration;
        }
        
        public static IConfigurationProperties UseSqlServer(this IConfigurationProperties configuration, string nameOrConnectionString, Action<SqlServer> configureSqlServer)
        {
            var sqlServer = new SqlServer(nameOrConnectionString);
            configureSqlServer(sqlServer);
            configuration.Datastores.Add(sqlServer);
            return configuration;
        }
    }
}