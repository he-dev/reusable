using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    public class AppSettingController : ConfigController
    {
        public AppSettingController(ControllerName name) : base(name) { }

        [ResourceGet]
        public Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = request.ResourceName;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];

            return
                element is {}
                    ? OK<ConfigResponse>(element.Value).ToTask<Response>()
                    : NotFound<ConfigResponse>().ToTask<Response>();
        }

        [ResourcePut]
        public Task<Response> SetSettingAsync(Request request)
        {
            var settingIdentifier = request.ResourceName;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            var value = Converter.Convert(request.Body, typeof(string));

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingIdentifier, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return OK<ConfigResponse>().ToTask<Response>();
        }

        [CanBeNull]
        private static string FindActualKey(Configuration exeConfig, string key)
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