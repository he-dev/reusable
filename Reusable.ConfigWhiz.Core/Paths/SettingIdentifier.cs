using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.ConfigWhiz.Extensions;

namespace Reusable.ConfigWhiz.Paths
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class SettingIdentifier : Identifier, IFormattable
    {
        public SettingIdentifier(IEnumerable<string> consumerNamespace, string consumer, string instance, string container, string settingName, string elementName)
            : base(
                 consumerNamespace.ToImmutableList(),
                 consumer,
                 instance,
                 container
            )
        {
            SettingName = settingName;
            ElementName = elementName;
        }

        protected SettingIdentifier(Type consumerType, string instance, PropertyInfo property, string elementName)
            : base(
                context: consumerType.Namespace.Split('.'),
                consumerType: consumerType,
                instance: instance,
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


        //public static SettingIdentifier Create(Type consumerType, string instanceName, PropertyInfo property, string elementName)
        //{
        //    return new SettingIdentifier(consumerType, instanceName, property, elementName);
        //}

        public static SettingIdentifier Create(Identifier identifier, PropertyInfo property, string elementName)
        {
            return new SettingIdentifier(
                identifier.Context,
                identifier.Consumer,
                identifier.Instance,
                property.DeclaringType.GetCustomNameOrDefault(),
                property.GetCustomNameOrDefault(),
                elementName
            );
        }

        public static SettingIdentifier Parse(string value)
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
                new SettingIdentifier(
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