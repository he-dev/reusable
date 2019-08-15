using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Controllers
{
    public class AppSettingController : SettingController
    {
        public AppSettingController() : base(ImmutableContainer.Empty) { }

        public ITypeConverter ResourceConverter { get; set; } = new NullConverter();

        [ResourceGet]
        public Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];            

            var result =
                element is null
                    ? new Response.NotFound()
                    : new Response.OK
                    {
                        Body = element.Value.to,
                        request
                            .Metadata
                            .Copy<ResourceProperties>()
                            .SetItem(SettingControllerProperties.Converter, ResourceConverter)
                            .SetItem(ResourceProperties.ActualName, settingIdentifier)
                    };

            return result.ToTask();
        }

        [ResourcePut]
        public async Task<IResource> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            var value = ResourceConverter.Convert(request.Body, typeof(string));

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingIdentifier, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return await GetSettingAsync(request);
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