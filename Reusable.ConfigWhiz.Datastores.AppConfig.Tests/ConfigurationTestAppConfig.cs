using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores.AppConfig;
using Reusable.ConfigWhiz.Tests;
using Reusable.ConfigWhiz.Tests.Common;

namespace Reusable.ConfigWhiz.Datastores.Tests
{
    [TestClass]
    public class ConfigurationTestAppConfig : ConfigurationTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Datastores = new IDatastore[]
            {
                new AppSettings("AppSetting1"), 
            };

            ResetData();
        }

        private static void ResetData()
        {
            var exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            exeConfiguration.AppSettings.Settings.Clear();
            exeConfiguration.ConnectionStrings.ConnectionStrings.Clear();

            foreach (var setting in SettingFactory.ReadSettings())
            {
                exeConfiguration.AppSettings.Settings.Add(setting.Id.ToString(), setting.Value.ToString());
            }            

            exeConfiguration.Save(ConfigurationSaveMode.Minimal);
        }
    }
}
