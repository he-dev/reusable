using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    public class AppSettingController : ConfigController
    {
        public override Task<Response> ReadAsync(ConfigRequest request)
        {
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, request.ResourceName) ?? request.ResourceName;
            var element = exeConfig.AppSettings.Settings[actualKey];

            return
                element is {}
                    ? Success<ConfigResponse>(request.ResourceName, element.Value).ToTask<Response>()
                    : NotFound<ConfigResponse>(request.ResourceName).ToTask<Response>();
        }

        public override Task<Response> CreateAsync(ConfigRequest request)
        {
            var settingIdentifier = request.ResourceName;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            var value = Converter.ConvertOrDefault(request.Body, typeof(string));

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingIdentifier, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return Success<ConfigResponse>(request.ResourceName).ToTask<Response>();
        }

        private static string? FindActualKey(Configuration exeConfig, string key)
        {
            return
                exeConfig
                    .AppSettings
                    .Settings
                    .AllKeys
                    .FirstOrDefault(k => SoftString.Comparer.Equals(k, key));
        }
    }
}