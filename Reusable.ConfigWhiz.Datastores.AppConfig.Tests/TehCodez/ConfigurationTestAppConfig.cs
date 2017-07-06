﻿using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Datastores.AppConfig;
using Reusable.SmartConfig.Tests;
using Reusable.SmartConfig.Tests.Common;

namespace Reusable.SmartConfig.Datastores.Tests
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