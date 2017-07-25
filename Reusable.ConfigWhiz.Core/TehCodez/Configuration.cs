using System;
using System.Collections.Generic;
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
        T GetValue<T>([NotNull] CaseInsensitiveString settingName, [CanBeNull] CaseInsensitiveString datasourceName = null);

        [CanBeNull]
        IEntity GetValue([NotNull] CaseInsensitiveString settingName, [CanBeNull] CaseInsensitiveString datasourceName = null);

        void SaveValue([NotNull] CaseInsensitiveString settingName, [NotNull] object value);
    }

    public static class MemberSetter
    {
        public static void SetValue<T>([NotNull] this Expression<Func<T>> expression, object value)
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

        public static IConfiguration SetValue<T>(this IConfiguration configuration, Expression<Func<T>> expression)
        {
            var value = configuration.GetValue(expression);



            expression.SetValue(value);
            return configuration;
        }

        public static T GetValue<T>(this IConfiguration config, Expression<Func<T>> expression)
        {
            var name = $"{expression.ToRootName()}";

            var smartConfig = expression.GetSmartSettingAttribute();

            name = smartConfig?.Name ?? name;

            var setting = config.GetValue(name, smartConfig?.Datasource);

            return default(T);
        }

        public static T GetValue<T>(this IConfiguration config, Expression<Func<T>> expression, string instance)
        {
            var name = $"{expression.ToRootName()}{InstanceSeparator}{instance}";
            return config.GetValue<T>(name);
        }

        public static Configuration AddValidation<T>(this Configuration config, Expression<Func<T>> expression, params ValidationAttribute[] validations)
        {
            var name = $"{expression.ToRootName()}";
            return config.AddValidation(name, validations);
        }

        private static string ToRootName<T>(this Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");

            // Namespace+Object.Property
            return
                $"{memberExpr.Member.DeclaringType.Namespace}" +
                $"{NamespaceSeparator}" +
                $"{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}";
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

        object Serialize(object value);
    }

    public class JsonSettingConverter : ISettingConverter
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSettingConverter()
        {
            
        }

        public T Deserialize<T>(object value)
        {
            switch (value)
            {
                case T x: return x;
                case string x: return JsonConvert.DeserializeObject<T>(x, _settings);
                default: throw new ArgumentException($"Unsupported value type '{typeof(T).Name}'.");
            }
        }

        public object Serialize(object value)
        {
            return null;
        }
    }

    public class Configuration : IConfiguration
    {
        private readonly IList<IDatastore> _datastores;

        // <actual-name, datastore>
        private readonly IDictionary<CaseInsensitiveString, IDatastore> _settingDatastores = new Dictionary<CaseInsensitiveString, IDatastore>();

        private readonly IDictionary<CaseInsensitiveString, IEnumerable<ValidationAttribute>> _settingValidations = new Dictionary<CaseInsensitiveString, IEnumerable<ValidationAttribute>>();

        // <full-name, actual-name>
        private readonly IDictionary<CaseInsensitiveString, CaseInsensitiveString> _actualNames = new Dictionary<CaseInsensitiveString, CaseInsensitiveString>();

        private JsonSerializerSettings _jsonSerializerSettings;

        public Configuration(params IDatastore[] datastores)
        {
            _datastores = datastores;
        }

        public Configuration(IEnumerable<IDatastore> datastores)
            : this(datastores.ToArray())
        { }

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        public JsonSerializerSettings JsonSerializerSettings
        {
            get => _jsonSerializerSettings;
            set => _jsonSerializerSettings = value ?? throw new ArgumentNullException(nameof(JsonSerializerSettings));
        }

        public Configuration AddValidation(CaseInsensitiveString name, params ValidationAttribute[] validations)
        {
            _settingValidations[name] = validations;
            return this;
        }

        public T GetValue<T>(CaseInsensitiveString settingName, CaseInsensitiveString datasourceName)
        {
            var names = settingName.GenerateNames();

            var setting =
                _datastores
                    .Select(datastore => datastore.Read(names))
                    .FirstOrDefault(Conditional.IsNotNull) ?? throw new SettingNotFoundException(names);

            //if(_actualNames)

            _actualNames[settingName] = setting.Name;

            switch (setting.Value)
            {
                case T value: return value;
                case string value: return JsonConvert.DeserializeObject<T>(value, _jsonSerializerSettings);
                default: throw new ArgumentException($"Unsupported value type '{typeof(T).Name}'.");
            }
        }

        public IEntity GetValue(CaseInsensitiveString settingName, CaseInsensitiveString datasourceName = null)
        {
            var names = settingName.GenerateNames();

            if (datasourceName == null)
            {
                return
                    _datastores
                        .Select(datastore => datastore.Read(names))
                        .FirstOrDefault(Conditional.IsNotNull);
            }
            else
            {
                var datastore = _datastores.SingleOrDefault(ds => ds.Name.Equals(datasourceName)) ?? throw new ArgumentException($"Datastore '{datasourceName.ToString()}' not found.");
                return datastore.Read(names);
            }
        }

        public void SaveValue(CaseInsensitiveString settingName, object value)
        {
            if (_actualNames.TryGetValue(settingName, out var actualName) && _settingDatastores.TryGetValue(actualName, out var datastore))
            {
                var setting = new Entity
                {
                    Name = actualName,
                    Value = datastore.CustomTypes.Contains(value.GetType()) ? value : JsonConvert.SerializeObject(value, _jsonSerializerSettings)
                };
                datastore.Write(setting);
            }
            else
            {
                throw new ArgumentException("Setting not initialized.");
            }
        }
    }

    [UsedImplicitly]
    public class SmartSettingAttribute : Attribute
    {
        // Name = "[member]" => the same as property/field
        public string Name { get; set; }

        public string Datasource { get; set; }

        public SettingNameLevel NameLevel { get; set; }
    }

    public enum SettingNameLevel
    {
        One,
        Two,
        Three
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
        public SettingNotFoundException(IEnumerable<CaseInsensitiveString> names)
            : base($"Could not find [{string.Join(", ", names.Select(n => n.ToString()))}]")
        { }
    }

    public class DuplicateDatatastoreException : Exception
    {
        public DuplicateDatatastoreException(IEnumerable<string> datastoreNames)
            : base($"Duplicate datastore names found: [{string.Join(", ", datastoreNames)}].")
        { }
    }
}
