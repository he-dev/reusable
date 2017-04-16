using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Reusable.ConfigWhiz.Data;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Datastores
{
    public class Registry : Datastore
    {
        private readonly RegistryKey _baseKey;
        private readonly string _baseSubKeyName;

        private readonly IReadOnlyDictionary<Type, RegistryValueKind> _registryValueKinds = new Dictionary<Type, RegistryValueKind>
        {
            { typeof(string), RegistryValueKind.String },
            { typeof(int), RegistryValueKind.DWord },
            { typeof(byte[]), RegistryValueKind.Binary },
        };

        public Registry(object handle, RegistryKey baseKey, string subKey)
            : base(handle, new[]
            {
                typeof(int),
                typeof(byte[]),
                typeof(string)
            })
        {
            _baseKey = baseKey ?? throw new ArgumentNullException(nameof(baseKey));
            _baseSubKeyName = subKey.NullIfEmpty() ?? throw new ArgumentNullException(nameof(subKey));
        }

        public override Result<IEnumerable<ISetting>> Read(SettingPath settingPath)
        {
            var subKeyName = Path.Combine(_baseSubKeyName, string.Join("\\", settingPath.ConsumerNamespace));
            using (var subKey = _baseKey.OpenSubKey(subKeyName, false))
            {
                if (subKey == null) return Result<IEnumerable<ISetting>>.Fail($"Could not open or create \"{_baseKey.Name}\\{_baseSubKeyName}\\{subKeyName}\".");

                var shortWeakPath = settingPath.ToShortWeakString();
                var settings =
                    from valueName in subKey.GetValueNames()
                    let valuePath = SettingPath.Parse(valueName)
                    where valuePath.ToShortWeakString().Equals(shortWeakPath, StringComparison.OrdinalIgnoreCase)
                    select new Setting
                    {
                        Path = valuePath,
                        Value = subKey.GetValue(valueName)
                    };
                return settings.ToList();
            }
        }

        public override Result Write(IGrouping<SettingPath, ISetting> settings)
        {
            void DeleteObsoleteSettings(RegistryKey registryKey)
            {
                var obsoleteNames =
                    from valueName in registryKey.GetValueNames()
                    where SettingPath.Parse(valueName).ToShortWeakString().Equals(settings.Key.ToShortWeakString(), StringComparison.OrdinalIgnoreCase)
                    select valueName;

                foreach (var obsoleteName in obsoleteNames) registryKey.DeleteValue(obsoleteName);
            }

            var subKeyName = Path.Combine(_baseSubKeyName, string.Join("\\", settings.Key.ConsumerNamespace));
            using (var subKey = _baseKey.OpenSubKey(subKeyName, true) ?? _baseKey.CreateSubKey(subKeyName))
            {
                if (subKey == null) return Result<IEnumerable<ISetting>>.Fail($"Could not open or create \"{_baseKey.Name}\\{_baseSubKeyName}\\{subKeyName}\".");

                DeleteObsoleteSettings(subKey);

                foreach (var setting in settings)
                {
                    if (!_registryValueKinds.TryGetValue(setting.Value.GetType(), out RegistryValueKind registryValueKind))
                    {
                        throw new InvalidTypeException(setting.Value.GetType(), SupportedTypes);
                    }

                    subKey.SetValue(setting.Path.ToShortStrongString(), setting.Value, registryValueKind);
                }
            }
            return Result.Ok();
        }

        //public const string DefaultKey = @"Software\SmartConfig";

        public static Registry CreateForCurrentUser(object handle, string subRegistryKey)
        {
            return new Registry(handle, Microsoft.Win32.Registry.CurrentUser, subRegistryKey);
        }

        public static Registry CreateForClassesRoot(object handle, string subRegistryKey)
        {
            return new Registry(handle, Microsoft.Win32.Registry.ClassesRoot, subRegistryKey);
        }

        public static Registry CreateForCurrentConfig(object handle, string subRegistryKey)
        {
            return new Registry(handle, Microsoft.Win32.Registry.CurrentConfig, subRegistryKey);
        }

        public static Registry CreateForLocalMachine(object handle, string subRegistryKey)
        {
            return new Registry(handle, Microsoft.Win32.Registry.LocalMachine, subRegistryKey);
        }

        public static Registry CreateForUsers(object handle, string subRegistryKey)
        {
            return new Registry(handle, Microsoft.Win32.Registry.Users, subRegistryKey);
        }
    }
}
