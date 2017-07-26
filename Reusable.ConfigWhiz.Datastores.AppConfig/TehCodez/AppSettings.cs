using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Datastores.AppConfig
{
    public class AppSettings : Datastore
    {
        //private readonly System.Configuration.Configuration _exeConfiguration;
        //private readonly AppSettingsSection _appSettingsSection;

        public AppSettings() : base(new[] { typeof(string) }) { }

        protected override ISetting ReadCore(IEnumerable<CaseInsensitiveString> names)
        {
            var exeConfig = OpenExeConfiguration();

            var result =
                (from n in names
                 let v = exeConfig.AppSettings.Settings[n.ToString()].Value
                 where !string.IsNullOrEmpty(v)
                 select (Name: n, Value: v)).FirstOrDefault();

            return result.Value.IsNullOrEmpty() ? null : new Setting
            {
                Name = result.Name,
                Value = result.Value
            };
        }

        protected override void WriteCore(ISetting setting)
        {
            var exeConfig = OpenExeConfiguration();

            if (exeConfig.AppSettings.Settings.AllKeys.Any(k => setting.Name.Equals(k)))
            {
                exeConfig.AppSettings.Settings[setting.ToString()].Value = setting.Value.ToString();
            }
            else
            {
                exeConfig.AppSettings.Settings.Add(setting.Name.ToString(), (string)setting.Value);
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);
        }

        private static System.Configuration.Configuration OpenExeConfiguration() => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }
}
