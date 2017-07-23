using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
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
        T GetValue<T>([NotNull] CaseInsensitiveString name);

        void Save([NotNull] CaseInsensitiveString name, [NotNull] object value);
    }

    public static class ConfigurationExtensions
    {
        public static T GetValue<T>(this IConfiguration config, Expression<Func<T>> expression)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            var name = $"{memberExpr.Member.DeclaringType.Namespace}+{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}";
            return config.GetValue<T>(name);
        }

        public static T GetValue<T>(this IConfiguration config, Expression<Func<T>> expression, string instance)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            var name = $"{memberExpr.Member.DeclaringType.Namespace}+{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}&{instance}";
            return config.GetValue<T>(name);
        }

        public static Configuration AddValidation<T>(this Configuration config, Expression<Func<T>> expression, params ValidationAttribute[] validations)
        {
            var memberExpr = expression.Body as MemberExpression ?? throw new ArgumentException("Expression must be a member expression.");
            var name = $"{memberExpr.Member.DeclaringType.Namespace}+{memberExpr.Member.DeclaringType.Name}.{memberExpr.Member.Name}";
            return config.AddValidation(name, validations);
        }
    }

    public class Configuration : IConfiguration
    {
        private readonly IList<IDatastore> _datastores = new List<IDatastore>();

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        // <actual-name, datastore>
        private readonly IDictionary<CaseInsensitiveString, IDatastore> _settingDatastores = new Dictionary<CaseInsensitiveString, IDatastore>();

        private readonly IDictionary<CaseInsensitiveString, IEnumerable<ValidationAttribute>> _settingValidations = new Dictionary<CaseInsensitiveString, IEnumerable<ValidationAttribute>>();

        // <full-name, actual-name>
        private readonly IDictionary<CaseInsensitiveString, CaseInsensitiveString> _actualNames = new Dictionary<CaseInsensitiveString, CaseInsensitiveString>();

        public Configuration(JsonSerializerSettings jsonSerializerSettings = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
        }

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use

        public Configuration AddDatastore(IDatastore datastore)
        {
            _datastores.Add(datastore);
            return this;
        }

        public Configuration AddValidation(CaseInsensitiveString name, params ValidationAttribute[] validations)
        {
            _settingValidations[name] = validations;
            return this;
        }

        public T GetValue<T>(CaseInsensitiveString name)
        {
            var names = name.GenerateNames();

            var setting =
                _datastores
                    .Select(datastore => datastore.Read(names))
                    .FirstOrDefault(Conditional.IsNotNull) ?? throw new DatastoreNotFoundException(names);

            //if(_actualNames)

            _actualNames[name] = setting.Name;

            switch (setting.Value)
            {
                case T value: return value;
                case string value: return JsonConvert.DeserializeObject<T>(value, _jsonSerializerSettings);
                default: throw new ArgumentException($"Unsupported value type '{typeof(T).Name}'.");
            }
        }

        public void Save(CaseInsensitiveString name, object value)
        {
            if (_actualNames.TryGetValue(name, out var actualName) && _settingDatastores.TryGetValue(actualName, out var datastore))
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

    public class DatastoreNotFoundException : Exception
    {
        public DatastoreNotFoundException(IEnumerable<CaseInsensitiveString> names)
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
