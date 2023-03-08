using System.Threading.Tasks;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class AppSettingControllerTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        public AppSettingControllerTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
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
            var env = await _testHelper.Resource.ReadSettingAsync(From<IProgramConfig>.Select(x => x.Environment));
            
            Assert.Equal("test", env);
        }
        
        //[ResourcePrefix("app")]
        //[ResourceName(Level = ResourceNameLevel.Member)]
        [UseScheme("app"), UseMember]
        [JoinSelectorTokens]
        private interface IProgramConfig
        {
            string Environment { get; }
        }
    }
}