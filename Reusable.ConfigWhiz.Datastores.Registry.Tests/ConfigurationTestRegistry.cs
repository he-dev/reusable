using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Tests;
using Reusable.Extensions;
using System.Linq;

namespace Reusable.ConfigWhiz.Datastores.Tests
{
    [TestClass]
    public class ConfigurationTestRegistry : ConfigurationTestDatastore
    {
        private const string TestRegistryKey = @"Software\ConfigWhiz\Tests";

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
                using (var subKey = baseKey.OpenSubKey(@"Software\ConfigWhiz", writable: true))
                {
                    if (subKey != null && subKey.GetSubKeyNames().Contains("Tests", StringComparer.OrdinalIgnoreCase))
                    {
                        subKey.DeleteSubKeyTree("Tests");
                    }
                }

                foreach (var setting in SettingFactory.ReadSettings<Foo>())
                {
                    var registryPath = setting.Path.ConsumerNamespace.Join("\\");
                    var subKeyName = Path.Combine(TestRegistryKey, registryPath);
                    using (var subKey = baseKey.CreateSubKey(subKeyName, writable: true))
                    {
                        subKey.SetValue(setting.Path.ToShortStrongString(), setting.Value);
                    }
                }
            }
        }
    }
}
