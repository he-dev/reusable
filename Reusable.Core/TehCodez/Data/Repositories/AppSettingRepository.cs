using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Data.Repositories
{
    public interface IAppSettingRepository
    {
        [NotNull, ItemNotNull]
        string[] AllKeys { get; }

        [CanBeNull]
        [ContractAnnotation("key: null => halt")]
        string GetValue([NotNull] string key);
    }

    public class AppSettingRepository : IAppSettingRepository
    {
        [NotNull]
        public static readonly IAppSettingRepository Default = new AppSettingRepository();

        public string[] AllKeys => ConfigurationManager.AppSettings.AllKeys;

        public string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return ConfigurationManager.AppSettings[key];
        }        
    }
}
