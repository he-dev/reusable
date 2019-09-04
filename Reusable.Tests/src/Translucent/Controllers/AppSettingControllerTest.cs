using System.Threading.Tasks;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class AppSettingControllerTest
    {
        public AppSettingControllerTest()
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
            var env = await TestHelper.Resources.ReadSettingAsync(From<IProgramConfig>.Select(x => x.Environment));
            
            Assert.Equal("test", env);
        }
        
        //[ResourcePrefix("app")]
        //[ResourceName(Level = ResourceNameLevel.Member)]
        [UseScheme("app"), UseMember]
        [PlainSelectorFormatter]
        private interface IProgramConfig
        {
            string Environment { get; }
        }
    }
}