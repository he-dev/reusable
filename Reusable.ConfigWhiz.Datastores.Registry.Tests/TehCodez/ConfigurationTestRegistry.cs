using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Datastores.TehCodez;
using Reusable.SmartConfig.Tests;
using Reusable.SmartConfig.Tests.Common;

namespace Reusable.SmartConfig.Datastores.Tests
{
    [TestClass]
    public class ConfigurationTestRegistry : ConfigurationTestBase
    {
        private const string TestRegistryKey = @"Software\SmartConfig\Tests";

        [TestInitialize]
        public void TestInitialize()
        {
            Datastores = new IDatastore[]
            {
                Registry.CreateForCurrentUser("Registry1", TestRegistryKey),
            };

            Utils.ResetData();
        }

        private static class Utils
        {
            public static void ResetData()
            {
                var baseKey = Microsoft.Win32.Registry.CurrentUser;
                using (var subKey = baseKey.OpenSubKey(@"Software\SmartConfig", writable: true))
                {
                    if (subKey != null && subKey.GetSubKeyNames().Contains("Tests", StringComparer.OrdinalIgnoreCase))
                    {
                        subKey.DeleteSubKeyTree("Tests");
                    }
                }

                foreach (var setting in SettingFactory.ReadSettings())
                {
                    //var registryPath = setting.Id.Context.Join("\\");
                    //var subKeyName = Path.Combine(TestRegistryKey, registryPath);
                    using (var subKey = baseKey.CreateSubKey(TestRegistryKey, writable: true))
                    {
                        subKey.SetValue(setting.Id.ToString(), setting.Value);
                    }
                }
            }
        }
    }
}
