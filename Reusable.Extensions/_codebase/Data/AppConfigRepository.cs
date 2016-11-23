using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Reusable.Validations;

namespace Reusable.Data
{
    public class AppConfigRepository
    {
        public string GetConnectionString(string name)
        {
            //name.Validate(nameof(name)).IsNotNullOrEmpty();

            var connectionStringSettings =
                ConfigurationManager.ConnectionStrings
                .Cast<ConnectionStringSettings>()
                .SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return connectionStringSettings?.ConnectionString;
        }

        public string GetAppSetting(string name)
        {
            //name.Validate(nameof(name)).IsNotNullOrEmpty();

            var value = ConfigurationManager.AppSettings[name];
            return value;
        }

        public string[] AppSettingsKeys => ConfigurationManager.AppSettings.AllKeys;
    }
}
