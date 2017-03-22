using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Data
{
    public class AppConfigRepository
    {
        public string GetConnectionString(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var connectionStringSettings =
                ConfigurationManager.ConnectionStrings
                .Cast<ConnectionStringSettings>()
                .SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return connectionStringSettings?.ConnectionString;
        }

        public string GetAppSetting(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var value = ConfigurationManager.AppSettings[name];
            return value;
        }

        public string[] AppSettingsKeys => ConfigurationManager.AppSettings.AllKeys;
    }
}
