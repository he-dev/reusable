using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Collections;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Services;
using Reusable.TypeConversion;

namespace Reusable.SmartConfig
{
    public interface IConfiguration
    {
        [CanBeNull]
        T Select<T>([NotNull] CaseInsensitiveString settingName, [CanBeNull] CaseInsensitiveString datasourceName = null, [CanBeNull] object defaultValue = null);

        void Update([NotNull] CaseInsensitiveString settingName, [NotNull] object value);
    }

    public class Configuration : IConfiguration
    {
        private readonly IList<IDatastore> _datastores;

        private readonly IDictionary<CaseInsensitiveString, (CaseInsensitiveString ActualName, IDatastore Datastore)> _settings = new Dictionary<CaseInsensitiveString, (CaseInsensitiveString ActualName, IDatastore Datastore)>();

        private ISettingConverter _settingConverter = new JsonSettingConverter();

        public Configuration(params IDatastore[] datastores)
        {
            _datastores = datastores;
        }

        public Configuration([NotNull] IEnumerable<IDatastore> datastores)
            : this(datastores.ToArray())
        {
            if (datastores == null) { throw new ArgumentNullException(nameof(datastores)); }
        }

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        [NotNull]
        public ISettingConverter SettingConverter
        {
            get => _settingConverter;
            set => _settingConverter = value ?? throw new ArgumentNullException(nameof(SettingConverter));
        }

        public T Select<T>(CaseInsensitiveString settingName, CaseInsensitiveString datasourceName = null, object defaultValue = null)
        {
            var setting = GetSetting(settingName, datasourceName);
            return _settingConverter.Deserialize<T>(setting.Value ?? defaultValue);
        }

        private ISetting GetSetting(CaseInsensitiveString settingName, CaseInsensitiveString datasourceName = null)
        {
            var names = settingName.GenerateNames();

            var result =
                (from ds in _datastores.Where(ds => datasourceName == null || ds.Name.Equals(datasourceName))
                 select (Datastore: ds, Setting: ds.Read(names))).FirstOrDefault(t => t.Setting.IsNotNull());

            _settings[settingName] = (result.Setting?.Name, result.Datastore ?? throw new SettingNotFoundException(settingName));
            return result.Setting;
        }

        public void Update(CaseInsensitiveString settingName, object value)
        {
            if (_settings.TryGetValue(settingName, out var t))
            {
                var setting = new Setting
                {
                    Name = t.ActualName,
                    Value = _settingConverter.Serialize(value, t.Datastore.CustomTypes)
                };
                t.Datastore.Write(setting);
            }
            else
            {
                throw new ArgumentException($"Setting '{settingName.ToString()}' not initialized.");
            }
        }

        //public static Configuration Validate<TObject, TValue>(Expression<Func<TObject, TValue>> expression, params ValidationAttribute[] validations)
        //{
        //    var name = $"{expression.CreateName()}";
        //    return config.AddValidation(name, validations);
        //}
    }

    //public static class CaseInsensitiveStringExtensions
    //{
    //    public static string ToJson(this IEnumerable<CaseInsensitiveString> names)
    //    {
    //        return $"[{string.Join(", ", names.Select(name => name.ToString()))}]";
    //    }
    //}

    public class BackingFieldNotFoundException : Exception
    {
        public BackingFieldNotFoundException(string propertyName)
            : base($"Property {propertyName} does not have a backing field.")
        { }
    }

    public class SettingNotFoundException : Exception
    {
        public SettingNotFoundException(CaseInsensitiveString name)
            : base($"Could not find '{name.ToString()}'.")
        { }
    }

    public class DuplicateDatatastoreException : Exception
    {
        public DuplicateDatatastoreException(IEnumerable<string> datastoreNames)
            : base($"Duplicate datastore names found: [{string.Join(", ", datastoreNames)}].")
        { }
    }
}
