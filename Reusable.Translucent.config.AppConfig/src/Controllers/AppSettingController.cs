using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;

namespace Reusable.Translucent.Controllers
{
    public class AppSettingController : ConfigController
    {
        public AppSettingController() : base(ImmutableContainer.Empty.SetItem(Resource.Converter, new EchoConverter())) { }

        [ResourceGet]
        public Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];

            return
                element is null
                    ? NotFound().ToTask()
                    : OK(request, element.Value, settingIdentifier).ToTask();
        }

        [ResourcePut]
        public Task<Response> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            var value = Properties.GetItem(Resource.Converter).Convert(request.Body, typeof(string));

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingIdentifier, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return OK().ToTask();
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

        #region Properties

        

        #endregion
    }
}