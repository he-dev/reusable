namespace Reusable.Tests.XUnit.SmartConfig.Providers
{
    public class AppSettingProviderTest
    {
        private static void SeedAppSettings()
        {

            var exeConfiguration = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

            exeConfiguration.AppSettings.Settings.Clear();
            //exeConfiguration.ConnectionStrings.ConnectionStrings.Clear();
            
            var data = new (string Key, string Value)[]
            {
                ("day:Greeting", "Hi!"),
            };

            foreach (var (key, value) in data)
            {
                exeConfiguration.AppSettings.Settings.Add(key, value);
            }

            exeConfiguration.Save(System.Configuration.ConfigurationSaveMode.Minimal);
        }
    }
}