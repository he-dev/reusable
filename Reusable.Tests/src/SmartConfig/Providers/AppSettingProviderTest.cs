using System.Threading.Tasks;
using Reusable.Data;
using Reusable.SmartConfig;
using Reusable.Tests.Foggle;
using Xunit;

namespace Reusable.Tests.SmartConfig.Providers
{
    public class AppSettingProviderTest
    {
        public AppSettingProviderTest()
        {
            SeedAppSettings();
        }
        
        private static void SeedAppSettings()
        {
            var exeConfiguration = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

            exeConfiguration.AppSettings.Settings.Clear();
            //exeConfiguration.ConnectionStrings.ConnectionStrings.Clear();
            
            var data = new (string Key, string Value)[]
            {
                ("app:Environment", "test"),
            };

            foreach (var (key, value) in data)
            {
                exeConfiguration.AppSettings.Settings.Add(key, value);
            }

            exeConfiguration.Save(System.Configuration.ConfigurationSaveMode.Minimal);
        }

        [Fact]
        public async Task Can_get_setting()
        {
            var c = new Configuration<IProgramConfig>(new AppSettingProvider());
            var env = await c.GetItemAsync(x => x.Environment);
            
            Assert.Equal("test", env);
        }
        
        //[ResourcePrefix("app")]
        //[ResourceName(Level = ResourceNameLevel.Member)]
        [UseGlobal("app"), UseMember]
        [SettingSelectorFormatter]
        internal interface IProgramConfig
        {
            string Environment { get; }
        }
    }
}