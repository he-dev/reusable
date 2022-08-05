using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

public abstract class AppConfigService<T> : ConfigService<T> where T : IRequest { }

public static class AppConfigServiceService
{
    public class Read : AppConfigService<IReadSetting>
    {
        protected override Task<object> InvokeAsync(IReadSetting setting)
        {
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return
                FindKeyOrDefault(exeConfig, setting.Name) is { } key
                    ? exeConfig.AppSettings.Settings[key].ToTask<object>()
                    : Unit.Default.ToTask<object>();
        }
    }

    public class Write : AppConfigService<IWriteSetting>
    {
        protected override Task<object> InvokeAsync(IWriteSetting setting)
        {
            if (setting.Value is not string value) throw new ArgumentException("Setting's value must be a string.");

            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (FindKeyOrDefault(exeConfig, setting.Name) is { } key)
            {
                exeConfig.AppSettings.Settings[key].Value = value;
            }
            else
            {
                exeConfig.AppSettings.Settings.Add(setting.Name, value);
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return Unit.Default.ToTask<object>();
        }
    }

    // Search for the key ignoring case.
    private static string? FindKeyOrDefault(Configuration exeConfig, string key)
    {
        return
            exeConfig
                .AppSettings
                .Settings
                .AllKeys
                .FirstOrDefault(k => SoftString.Comparer.Equals(k, key));
    }
}