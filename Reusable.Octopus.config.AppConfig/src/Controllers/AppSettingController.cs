using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers;

public class AppSettingController : ConfigController
{
    public override Task<Response> ReadAsync(ConfigRequest request)
    {
        var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var actualKey = FindActualKey(exeConfig, request.ResourceName.Peek()) ?? request.ResourceName.Peek();
        var element = exeConfig.AppSettings.Settings[actualKey];
        
        request.ResourceName.Push(actualKey);

        return
            element is { }
                ? Success<ConfigResponse>(request.ResourceName, element.Value).ToTask()
                : NotFound<ConfigResponse>(request.ResourceName).ToTask();
    }

    public override Task<Response> CreateAsync(ConfigRequest request)
    {
        var settingIdentifier = request.ResourceName.Peek();
        var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
        var element = exeConfig.AppSettings.Settings[actualKey];
        var value = (string)request.Body.Peek(); // Converter.ConvertOrThrow<string>(request.Body!);

        request.ResourceName.Push(actualKey);
        
        if (element is { })
        {
            exeConfig.AppSettings.Settings[actualKey].Value = value;
        }
        else
        {
            exeConfig.AppSettings.Settings.Add(settingIdentifier, value);
        }

        exeConfig.Save(ConfigurationSaveMode.Minimal);

        return Success<ConfigResponse>(request.ResourceName).ToTask();
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