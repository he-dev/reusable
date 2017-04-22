using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Datastores.AppConfig;
using Reusable.ConfigWhiz.Tests;

namespace Reusable.ConfigWhiz.Datastores.Tests
{
    [TestClass]
    public class ConfigurationTestAppConfig : ConfigurationTestDatastore
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

            foreach (var setting in SettingFactory.ReadSettings<Foo>())
            {
                exeConfiguration.AppSettings.Settings.Add(setting.Path.ToFullStrongString(), setting.Value.ToString());
            }            

            exeConfiguration.Save(ConfigurationSaveMode.Minimal);
        }
    }
}
