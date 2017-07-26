using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.SmartConfig.Collections;
using Reusable.SmartConfig.Data;
using Reusable.TypeConversion;

namespace Reusable.SmartConfig
{
    public interface IConfiguration
    {
        [CanBeNull]
        T Select<T>([NotNull] CaseInsensitiveString settingName, [CanBeNull] CaseInsensitiveString datasourceName = null, [CanBeNull] object defaultValue = null);

        void Update([NotNull] CaseInsensitiveString settingName, [NotNull] object value);
    }

    public static class MemberSetter
    {
        public static void Apply<T>([NotNull] this Expression<Func<T>> expression, [CanBeNull] object value)
        {
            if (expression == null) { throw new ArgumentNullException(nameof(expression)); }
            if (expression.Body is MemberExpression memberExpression)
            {
                var obj = GetObject(memberExpression.Expression);

                switch (memberExpression.Member.MemberType)
                {
                    case MemberTypes.Property:
                        var property = (PropertyInfo)memberExpression.Member;
                        if (property.CanWrite)
                        {
                            ((PropertyInfo)memberExpression.Member).SetValue(obj, value);
                        }
                        else
                        {
                            var bindingFlags = BindingFlags.NonPublic | (obj == null ? BindingFlags.Static : BindingFlags.Instance);
                            var backingField = (obj?.GetType() ?? property.DeclaringType).GetField($"<{property.Name}>k__BackingField", bindingFlags);
                            if (backingField == null)
                            {
                                throw new BackingFieldNotFoundException(property.Name);
                            }
                            backingField.SetValue(obj, value);
                        }
                        break;
                    case MemberTypes.Field:
                        ((FieldInfo)memberExpression.Member).SetValue(obj, value);
                        break;
                    default:
                        throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
                }
            }
            else
            {
                throw new ArgumentException($"Expression must be a {nameof(MemberExpression)}.");
            }
        }

        private static object GetObject(Expression expression)
        {
            // This is a static class.
            if (expression == null)
            {
                return null;
            }
            if (expression is MemberExpression anonymousMemberExpression)
            {
                // Extract constant value from the anonyous-wrapper
                var container = ((ConstantExpression)anonymousMemberExpression.Expression).Value;
                return ((FieldInfo)anonymousMemberExpression.Member).GetValue(container);
            }
            else
            {
                return ((ConstantExpression)expression).Value;
            }
        }
    }

    public class BackingFieldNotFoundException : Exception
    {
        public BackingFieldNotFoundException(string propertyName)
            : base($"Property {propertyName} does not have a backing field.")
        { }
    }

    public static class ConfigurationExtensions
    {
        private const string NamespaceSeparator = "+";

        private const string InstanceSeparator = ",";

        public static IConfiguration Apply<T>(this IConfiguration configuration, Expression<Func<T>> expression, string instance = null)
        {
            var value = configuration.Select(expression, instance);
            expression.Apply(value);
            return configuration;
        }

        public static T Select<T>(this IConfiguration config, Expression<Func<T>> expression, string instance = null)
        {
            var smartConfig = expression.GetSmartSettingAttribute();
            var name = smartConfig?.Name ?? expression.CreateName(instance);
            var setting = config.Select<T>(name, smartConfig?.Datasource, expression.GetDefaultValue());            
            return setting;
        }

        //public static Configuration AddValidation<T>(this Configuration config, Expression<Func<T>> expression, params ValidationAttribute[] validations)
        //{
        //    var name = $"{expression.CreateName()}";
        //    return config.AddValidation(name, validations);
        //}

        private static CaseInsensitiveString CreateName<T>(this Expression<Func<T>> expression, string instance)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");

            // Namespace+Object.Property
            return
                $"{memberExpr.Member.DeclaringType.Namespace}" +
                $"{NamespaceSeparator}" +
                $"{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}" +
                (string.IsNullOrEmpty(instance) ? string.Empty : $"{InstanceSeparator}{instance}");
        }

        private static SmartSettingAttribute GetSmartSettingAttribute<T>(this Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            return memberExpr.Member.GetCustomAttribute<SmartSettingAttribute>();
        }

        private static object GetDefaultValue<T>(this Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            return memberExpr.Member.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        }
    }

    public interface ISettingConverter
    {
        T Deserialize<T>(object value);

        object Serialize(object value, IImmutableSet<Type> customTypes);
    }

    public abstract class SettingConverter : ISettingConverter
    {
        public T Deserialize<T>(object value)
        {
            if (value == null) return default(T);
            return (value is T x) ? x : DeserializeCore<T>(value);
        }

        protected abstract T DeserializeCore<T>(object value);

        public object Serialize(object value, IImmutableSet<Type> customTypes)
        {
            if (value == null) return null;
            return customTypes.Contains(value.GetType()) ? value : SerializeCore(value);
        }

        protected abstract object SerializeCore(object value);
    }

    public class JsonSettingConverter : SettingConverter
    {
        public JsonSettingConverter(JsonSerializerSettings settings = null)
        {
            JsonSerializerSettings = settings ?? new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public JsonSerializerSettings JsonSerializerSettings { get; }

        protected override T DeserializeCore<T>(object value)
        {
            if (value is string s)
            {
                return JsonConvert.DeserializeObject<T>(s, JsonSerializerSettings);
            }
            throw new ArgumentException($"Unsupported type '{typeof(T).Name}'.");
        }

        protected override object SerializeCore(object value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }
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

        public Configuration(IEnumerable<IDatastore> datastores)
            : this(datastores.ToArray())
        { }

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        public ISettingConverter SettingConverter
        {
            get => _settingConverter;
            set => _settingConverter = value ?? throw new ArgumentNullException(nameof(SettingConverter));
        }
        
        public T Select<T>(CaseInsensitiveString settingName, CaseInsensitiveString datasourceName, object defaultValue)
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
    }

    [UsedImplicitly]
    public class SmartSettingAttribute : Attribute
    {
        // Name = "[member]" => the same as property/field
        public CaseInsensitiveString Name { get; set; }

        public CaseInsensitiveString Datasource { get; set; }
    }

    public static class NameGenerator
    {
        // language=regexp
        private const string NamePattern = @"(?:(?<Namespace>[a-z0-9_.]+)\+)?(?:(?<Type>[a-z0-9_]+)\.)?(?<Name>[a-z0-9_]+)(?:&(?<Instance>[a-z0-9_]+))?";

        public static IEnumerable<CaseInsensitiveString> GenerateNames([NotNull] this CaseInsensitiveString name)
        {

            /*
            
            Paths in order of resolution
            
            Program.Environment&Foo
            Environment&Foo
            TheApp+Program.Environment&Foo

            Program.Environment
            Environment
            TheApp+Program.Environment

             */

            var match = Regex.Match(name.ToString(), NamePattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid name.");
            }

            if (match.Groups["Instance"].Success)
            {
                yield return $"{match.Groups["Type"].Value}.{match.Groups["Name"].Value}&{match.Groups["Instance"].Value}";
                yield return $"{match.Groups["Name"].Value}&{match.Groups["Instance"].Value}";
                yield return $"{match.Groups["Namespace"].Value}+{match.Groups["Type"].Value}.{match.Groups["Name"].Value}&{match.Groups["Instance"].Value}";
            }

            yield return $"{match.Groups["Type"].Value}.{match.Groups["Name"].Value}";
            yield return $"{match.Groups["Name"].Value}";
            yield return $"{match.Groups["Namespace"].Value}+{match.Groups["Type"].Value}.{match.Groups["Name"].Value}";
        }
    }

    public static class CaseInsensitiveStringExtensions
    {
        public static string ToJson(this IEnumerable<CaseInsensitiveString> names)
        {
            return $"[{string.Join(", ", names.Select(name => name.ToString()))}]";
        }
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
