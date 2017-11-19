namespace Reusable.SmartConfig
{
    public static class AppSettingsConfigurationOptionsExtensions
    {
        public static IConfigurationProperties UseAppSettings(this IConfigurationProperties properties)
        {
            properties.Datastores.Add(new AppSettings());
            return properties;
        }
    }
}