using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Collections;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.ConfigWhiz.Extensions;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz
{
    public class ContainerPath : IEquatable<ContainerPath>
    {
        protected ContainerPath(IEnumerable<string> consumerNamespace, string consumerName, string instanceName, string containerName)
        {
            ConsumerNamespace = consumerNamespace.ToImmutableList();
            ConsumerName = consumerName;
            InstanceName = instanceName;
            ContainerName = containerName;
        }

        protected ContainerPath(IEnumerable<string> consumerNamespace, Type consumerType, string instanceName, Type containerType) 
            : this(
                  consumerNamespace, 
                  consumerType.GetCustomNameOrDefault(), 
                  instanceName, 
                  containerType.GetCustomNameOrDefault())
        {
        }

        public IImmutableList<string> ConsumerNamespace { get; }

        public string ConsumerName { get; }

        public string InstanceName { get; }

        public string ContainerName { get; }

        public static ContainerPath Create<TConsumer, TContainer>(string instanceName)
        {
            // ReSharper disable once PossibleNullReferenceException
            return new ContainerPath(
                consumerNamespace: typeof(TConsumer).Namespace.Split('.').ToImmutableList(),
                consumerType: typeof(TConsumer),
                instanceName: instanceName,
                containerType: typeof(TContainer));
        }

        #region IEquatable<ContainerPath>

        public bool Equals(ContainerPath other)
        {
            return this == other;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj as ContainerPath);
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
                yield return ConsumerNamespace.Join(".").GetHashCode();
                yield return ConsumerName.GetHashCode();
                yield return InstanceName?.GetHashCode() ?? 0;
                yield return ConsumerName.GetHashCode();
            }
        }

        public static bool operator ==(ContainerPath left, ContainerPath right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.ConsumerNamespace.SequenceEqual(right.ConsumerNamespace, StringComparer.OrdinalIgnoreCase) &&
                left.ConsumerName.Equals(right.ConsumerName, StringComparison.OrdinalIgnoreCase) &&
                (left.InstanceName.IsNotNullOrEmpty() && right.InstanceName.IsNotNullOrEmpty() && left.InstanceName.Equals(right.InstanceName, StringComparison.OrdinalIgnoreCase)) &&
                left.ContainerName.Equals(right.ContainerName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(ContainerPath left, ContainerPath right)
        {
            return !(left == right);
        }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class SettingPath : ContainerPath, IFormattable
    {
        public SettingPath(IEnumerable<string> consumerNamespace, string consumerName, string instanceName, string containerName, string settingName, string elementName)
            : base(
                 consumerNamespace.ToImmutableList(),
                 consumerName,
                 instanceName,
                 containerName
            )
        {
            SettingName = settingName;
            ElementName = elementName;
        }

        protected SettingPath(Type consumerType, string instanceName, PropertyInfo property, string elementName)
            : base(
                consumerNamespace: consumerType.Namespace.Split('.'),
                consumerType: consumerType,
                instanceName: instanceName,
                containerType: property.DeclaringType
            )
        {
            SettingName = property.GetCustomNameOrDefault();
            ElementName = elementName;
        }

        public string SettingName { get; }

        public string ElementName { get; }

        private string DebuggerDisplay => this.ToFullStrongString();

        //new string[] {
        //    $"{nameof(ConsumerNamespace)} = \"{string.Join(", ", ConsumerNamespace)}\"",
        //    $"{nameof(ConsumerName)} = \"{ConsumerName}\"",
        //    $"{nameof(InstanceName)} = \"{InstanceName}\"",
        //    $"{nameof(SettingName)} = \"{SettingName}\"",
        //    $"{nameof(ElementName)} = \"{ElementName}\""
        //}.Join(" ");


        public static SettingPath Create(Type consumerType, string instanceName, PropertyInfo property, string elementName)
        {
            return new SettingPath(consumerType, instanceName, property, elementName);
        }

        public static SettingPath Create(ContainerPath containerPath, PropertyInfo property, string elementName)
        {
            return new SettingPath(
                containerPath.ConsumerNamespace,
                containerPath.ConsumerName,
                containerPath.InstanceName,
                property.DeclaringType.GetCustomNameOrDefault(),
                property.GetCustomNameOrDefault(),
                elementName
            );
        }

        public static SettingPath Parse(string value)
        {
            var matches = Regex.Matches(value, @"(?<Delimiter>[^a-z0-9_])?(?<Name>[a-z_][a-z0-9_]*)(?:\[(?<Key>"".+?""|'.+?'|.+?)\])?", RegexOptions.IgnoreCase);

            var items =
                (from m in matches.Cast<Match>()
                 select (
                 Delimiter: m.Groups["Delimiter"].Value,
                 Name: m.Groups["Name"].Value,
                 Key: m.Groups["Key"].Value.Trim('"', '\'')
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
                new SettingPath(
                    consumerNamespace,
                    consumerName,
                    instanceName,
                    containerName,
                    settingName,
                    elementName
                );
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return
                formatProvider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter x
                    ? x.Format(format, this, formatProvider)
                    : base.ToString();
        }
    }
}