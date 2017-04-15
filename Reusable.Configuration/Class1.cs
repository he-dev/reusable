using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.Collections;
using Reusable.Data.Annotations;
using Reusable.Drawing;
using Reusable.Extensions;
using Reusable.StringFormatting;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class Configuration
    {
        private readonly IImmutableList<IDatastore> _settingStores;
        private readonly AutoKeyDictionary<SettingContainerKey, SettingContainer> _containers = new AutoKeyDictionary<SettingContainerKey, SettingContainer>(x => x.Key);

        public Configuration(IImmutableList<IDatastore> settingStores)
        {
            _settingStores = settingStores;
        }

        public TContainer Resolve<TConsumer, TContainer>(TConsumer entity, Func<TConsumer, string> selectConsumerName) where TContainer : new() => Resolve<TConsumer, TContainer>(selectConsumerName(entity));

        public TContainer Resolve<TConsumer, TContainer>() where TContainer : new() => Resolve<TConsumer, TContainer>(string.Empty);

        private TContainer Resolve<TConsumer, TContainer>(string consumerName) where TContainer : new()
        {
            var key = SettingContainerKey.Create<TConsumer, TContainer>(consumerName);

            if (_containers.TryGetValue(key, out SettingContainer container) == false)
            {
                container = SettingContainer.Create<TConsumer, TContainer>(consumerName, _settingStores);
                _containers.Add(container);
            }
            //ImmutableDictionary<string, object>.Empty.Add()
            return container.As<TContainer>();
        }
    }

    public class SettingContainerKey
    {
        private SettingContainerKey(Type consumerType, string consumerName, Type containerType)
        {
            ConsumerType = consumerType;
            ConsumerName = consumerName;
            ContainerType = containerType;
        }

        public Type ConsumerType { get; }

        public string ConsumerName { get; }

        public Type ContainerType { get; }


        public static SettingContainerKey Create<TConsumer, TContainer>(string entityName)
        {
            return new SettingContainerKey(typeof(TConsumer), entityName, typeof(TContainer));
        }

        protected bool Equals(SettingContainerKey other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return
                !ReferenceEquals(null, obj) &&
                !ReferenceEquals(this, obj) &&
                obj is SettingContainerKey spk &&
                Equals(spk);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                //var hashCode = ConsumerType?.GetHashCode() ?? 0;
                //hashCode = (hashCode * 397) ^ (ConsumerName?.GetHashCode() ?? 0);
                //hashCode = (hashCode * 397) ^ (ContainerType?.GetHashCode() ?? 0);
                ////return hashCode;
                return GetHashCodes().Aggregate(0, (current, next) => (current * 397) ^ next);
            }

            IEnumerable<int> GetHashCodes()
            {
                yield return ConsumerType?.GetHashCode() ?? 0;
                yield return ConsumerName?.GetHashCode() ?? 0;
                yield return ConsumerType?.GetHashCode() ?? 0;
            }
        }

        public static bool operator ==(SettingContainerKey left, SettingContainerKey right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.ConsumerType == right.ConsumerType &&
                left.ConsumerName == right.ConsumerName &&
                left.ContainerType == right.ContainerType;
        }

        public static bool operator !=(SettingContainerKey left, SettingContainerKey right)
        {
            return !(left == right);
        }
    }

    public class SettingProxy
    {
        private readonly SettingContainerKey _containerKey;
        private readonly object _container;
        private readonly PropertyInfo _property;
        private readonly IImmutableList<IDatastore> _stores;
        private readonly TypeConverter _converter;
        private IDatastore _currentStore;

        public SettingProxy(object container, SettingContainerKey containerKey, PropertyInfo property, IImmutableList<IDatastore> stores, TypeConverter converter)
        {
            _container = container;
            _containerKey = containerKey;
            _property = property;
            _stores = stores;
            _converter = converter;
        }

        public IEnumerable<ValidationAttribute> ValidationAttributes { get; set; }

        private object Value
        {
            get => _property.GetValue(_container);
            set => _property.SetValue(_container, value);
        }

        public Result<SettingProxy, bool> Load()
        {
            var sw = Stopwatch.StartNew();

            // a.b - full-weak
            // App.Windows.MainWindow["Window1"].WindowDimensions.Height
            var path = new SettingPath(_containerKey.ConsumerType, _property)
            {
                //ConsumerName = _containerKey.ConsumerName
            };
            foreach (var store in _stores)
            {
                var data = store.Read(path);
                _currentStore = store;
                return Result<SettingProxy, bool>.Ok(this, true, sw.Elapsed);
            }

            // validate
            // convert
            // save

            return Result<SettingProxy, bool>.Ok(this, false, sw.Elapsed);
        }

        public Result<SettingProxy, bool> Save()
        {
            return Result<SettingProxy, bool>.Ok(this, true);
        }
    }

    public class SettingContainer
    {
        private readonly object _container;
        private readonly IImmutableList<SettingProxy> _proxies;

        public SettingContainer(object container, SettingContainerKey key, IImmutableList<SettingProxy> proxies)
        {
            _container = container;
            _proxies = proxies;
            Key = key;
        }

        public SettingContainerKey Key { get; }

        public TContainer As<TContainer>() => (TContainer)_container;

        public IImmutableList<Result<SettingProxy, bool>> Load()
        {
            return (from p in _proxies select p.Load()).ToImmutableList();
        }

        public IImmutableList<Result<SettingProxy, bool>> Save()
        {
            return (from p in _proxies select p.Save()).ToImmutableList();
        }

        public static SettingContainer Create<TConsumer, TContainer>(string containerName, IImmutableList<IDatastore> stores) where TContainer : new()
        {
            var container = new TContainer();
            var containerKey = SettingContainerKey.Create<TConsumer, TContainer>(containerName);

            var converter =
                typeof(TContainer)
                    .GetCustomAttributes<TypeConverterAttribute>()
                    .Aggregate(
                        TypeConverterFactory.CreateDefaultConverter(),
                        (current, next) => current.Add(next.ConverterType));

            var properties =
                from property in typeof(TContainer).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetCustomAttribute<IgnoreAttribute>() == null
                select property;

            var proxies =
                from property in properties
                select new SettingProxy(container, containerKey, property, stores, converter);

            return new SettingContainer(container, containerKey, proxies.ToImmutableList());
        }
    }

    internal static class Namespace
    {
        public static IEnumerable<IEnumerable<string>> ExplodeNamespaces(this IEnumerable<string> namespaces)
        {
            return namespaces.Select(Split('.'));

            Func<string, IEnumerable<string>> Split(params char[] separators) => s => s.Split(separators);
        }

        public static IEnumerable<string> Split(string @namespace, params char[] separators) => @namespace.Split(separators);

        public static (IEnumerable<string> Common, IEnumerable<IEnumerable<string>> Distinct) ReduceNamespaces(this IEnumerable<IEnumerable<string>> namespaces)
        {
            var common = new List<string>();
            while (FirstEquals(namespaces, out string name))
            {
                common.Add(name);
                namespaces = namespaces.Select(SkipFirst());
            }

            return (common, namespaces);
        }

        public static IEnumerable<string> CommonNamespace(this IEnumerable<IEnumerable<string>> namespaces)
        {
            while (FirstEquals(namespaces, out string first))
            {
                yield return first;
                namespaces = namespaces.Select(SkipFirst());
            }
        }

        private static bool FirstEquals<T>(IEnumerable<IEnumerable<T>> values, out T first)
        {
            var distinct = values.Select(ns => ns.FirstOrDefault()).Distinct().ToList();
            switch (distinct.Count)
            {
                case 1:
                    first = distinct.Single();
                    return true;
                default:
                    first = default(T);
                    return false;
            }
        }

        private static Func<IEnumerable<string>, IEnumerable<string>> SkipFirst() => values => values.Skip(1);
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class SettingPath : IFormattable
    {
        public SettingPath(IImmutableList<string> consumerNamespace, string consumerName, string containerName, string settingName)
        {
            ConsumerNamespace = consumerNamespace;
            ConsumerName = consumerName;
            ContainerName = containerName;
            SettingName = settingName;
        }

        public SettingPath(Type consumerType, PropertyInfo property)
            : this(
                consumerNamespace: consumerType.Namespace.Split('.').ToImmutableList(),
                consumerName: consumerType.Name,
                containerName: property.DeclaringType.Name,
                settingName: property.Name
            )
        { }

        public IImmutableList<string> ConsumerNamespace { get; }

        public string ConsumerName { get; }

        public string InstanceName { get; set; }

        public string ContainerName { get; }

        public string SettingName { get; }

        public string ElementName { get; set; }

        private string DebuggerDisplay => new string[] {
            $"{nameof(ConsumerNamespace)} = \"{string.Join(", ", ConsumerNamespace)}\"",
            $"{nameof(ConsumerName)} = \"{ConsumerName}\"",
            $"{nameof(InstanceName)} = \"{InstanceName}\"",
            $"{nameof(SettingName)} = \"{SettingName}\"",
            $"{nameof(ElementName)} = \"{ElementName}\""
        }.Join(" ");

        public static SettingPath Parse(string value)
        {
            var matches = Regex.Matches(value, @"(?<Delimiter>[^a-z0-9_])?(?<Name>[a-z_][a-z0-9_]*)(?:\[(?<Key>"".+?""|any)\])?", RegexOptions.IgnoreCase);

            var items =
                (from m in matches.Cast<Match>()
                 select (
                     Delimiter: m.Groups["Delimiter"].Value,
                     Name: m.Groups["Name"].Value,
                     Key: m.Groups["Key"].Value
                 )).ToList();

            var containerNameCount = 2;
            var consumerNameCount = items.Count - containerNameCount;
            var isFullName = consumerNameCount > 0;

            var consumerNamespace = items.Take(consumerNameCount - 1).Select(x => x.Name).ToImmutableList();
            var consumerName = isFullName ? items.Skip(consumerNameCount - 1).Take(1).SingleOrDefault().Name : string.Empty;
            var instanceName = isFullName ? items.Skip(consumerNameCount - 1).Take(1).SingleOrDefault().Key : string.Empty;
            var containerName = items.Skip(consumerNameCount).Take(1).SingleOrDefault().Name;
            var settingName = items.Skip(consumerNameCount + 1).Take(1).SingleOrDefault().Name;
            var elementName = items.Skip(consumerNameCount + 1).Take(1).SingleOrDefault().Key;

            return
                new SettingPath(consumerNamespace, consumerName,  containerName, settingName)
                {
                    InstanceName = instanceName.Equals("Any", StringComparison.OrdinalIgnoreCase) ? string.Empty : instanceName,
                    ElementName = elementName
                };
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return
                formatProvider.GetFormat(typeof(SettingPathFormatter)) is ICustomFormatter x
                    ? x.Format(format, this, formatProvider)
                    : base.ToString();
        }
    }

    public class SettingPathFormat
    {
        private SettingPathFormat(string format) => Format = format;
        public string Format { get; }
        public static readonly SettingPathFormat ShortWeak = new SettingPathFormat(".a");
        public static readonly SettingPathFormat ShortStrong = new SettingPathFormat(".a[]");
        public static readonly SettingPathFormat FullWeak = new SettingPathFormat("a.b");
        public static readonly SettingPathFormat FullStrong = new SettingPathFormat("a.b[]");
        public static implicit operator string(SettingPathFormat format) => format.Format;
    }

    public class SettingPathFormatter : CustomFormatter
    {
        public static readonly SettingPathFormatter Instance = new SettingPathFormatter();

        // https://regex101.com/r/OC6uiH/1
        /*

            short
                weak - .a
                strong - .a[]
            full
                weak - a.b
                strong - a.b[]

         */

        // Margins - a
        // Margins["0"] - a[]
        // App.Windows.MainWindow["Window1"].WindowDimensions.Margins - a.b
        // App.Windows.MainWindow["Window1"].WindowDimensions.Margins["0"] - a.b[]

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            var match = Regex.Match(format, @"(?<full>[a-b])?((?<delimiter>.)?(?<short>[a-b])(?<strong>\[\])?)", RegexOptions.IgnoreCase);
            if (!match.Success || !(arg is SettingPath settingPath)) return null;

            var delimiter = match.Groups["delimiter"].Value;

            var name = new StringBuilder();

            if (match.Groups["full"].Success)
            {
                name
                    .Append(string.Join(delimiter, settingPath.ConsumerNamespace))
                    .Append(delimiter)
                    .Append(settingPath.ConsumerName)
                    .Append(settingPath.InstanceName.IsNullOrEmpty() ? "[Any]" : $"[\"{settingPath.InstanceName.Trim('"')}\"]")
                    .Append(delimiter);
            }

            name
                .Append(settingPath.ContainerName)
                .Append(delimiter)
                .Append(settingPath.SettingName);

            if (match.Groups["strong"].Success && settingPath.ElementName.IsNotNullOrEmpty())
            {
                name.Append($"[\"{settingPath.ElementName.Trim('"')}\"]");
            }

            return name.ToString();
        }
    }

    public static class SettingPathExtensions
    {
        private static readonly SettingNameComparer FullWeakSettingNameComparer = new SettingNameComparer(SettingPathFormat.FullWeak);

        public static bool IsLike(this SettingPath x, SettingPath y)
        {
            return FullWeakSettingNameComparer.Equals(x, y);
        }
    }

    public class SettingNameComparer : IEqualityComparer<SettingPath>
    {
        private static readonly SettingPathFormatter Formatter = new SettingPathFormatter();

        public SettingNameComparer(SettingPathFormat format)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
        }

        public SettingPathFormat Format { get; }

        public bool Equals(SettingPath x, SettingPath y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.ToString(Format, Formatter).Equals(y.ToString(Format, Formatter), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(SettingPath obj)
        {
            return obj.ToString(Format, Formatter).GetHashCode();
        }
    }

    public static class TypeConverterFactory
    {
        public static TypeConverter CreateDefaultConverter() => TypeConverter.Empty.Add(new TypeConverter[]
        {
            new StringToSByteConverter(),
            new StringToByteConverter(),
            new StringToCharConverter(),
            new StringToInt16Converter(),
            new StringToInt32Converter(),
            new StringToInt64Converter(),
            new StringToUInt16Converter(),
            new StringToUInt32Converter(),
            new StringToUInt64Converter(),
            new StringToSingleConverter(),
            new StringToDoubleConverter(),
            new StringToDecimalConverter(),
            new StringToColorConverter(new ColorParser[]
            {
                new NameColorParser(),
                new DecimalColorParser(),
                new HexadecimalColorParser(),
            }),
            new StringToBooleanConverter(),
            new StringToDateTimeConverter(),
            new StringToEnumConverter(),

            new SByteToStringConverter(),
            new ByteToStringConverter(),
            new CharToStringConverter(),
            new Int16ToStringConverter(),
            new Int32ToStringConverter(),
            new Int64ToStringConverter(),
            new UInt16ToStringConverter(),
            new UInt32ToStringConverter(),
            new UInt64ToStringConverter(),
            new SingleToStringConverter(),
            new DoubleToStringConverter(),
            new DecimalToStringConverter(),
            new ColorToStringConverter(),
            new BooleanToStringConverter(),
            new DateTimeToStringConverter(),
            new EnumToStringConverter(),

            new EnumerableToArrayConverter(),
            new EnumerableToListConverter(),
            new EnumerableToHashSetConverter(),
            new DictionaryToDictionaryConverter(),
        });
    }
}
