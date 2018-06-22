using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.SmartConfig.Data
{
    [PublicAPI]
    public partial class SettingName
    {
        public const string NamespaceSeparator = "+";
        public const string TypeSeparator = ".";
        public const string InstanceSeparator = ",";

        // [Namespace+][Type.]Property[,Instance]
        public static readonly string Format =
            $"[{nameof(Namespace)}{NamespaceSeparator}]" +
            $"[{nameof(Type)}{TypeSeparator}]" +
            $"{nameof(Property)}" +
            $"[{InstanceSeparator}{nameof(Instance)}]";

        private static readonly string NamePattern =
            $"(?:(?<Namespace>[a-z0-9_.]+)\\{NamespaceSeparator})?" +
            $"(?:(?<Type>[a-z0-9_]+)\\{TypeSeparator})?" +
            $"(?<Property>[a-z0-9_]+)" +
            $"(?:{InstanceSeparator}(?<Instance>[a-z0-9_]+))?";

        [NotNull]
        private SoftString _property;

        public SettingName([NotNull] SoftString property)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
        }

        public SettingName(SettingName settingName) : this(settingName.Property)
        {
            Namespace = settingName.Namespace;
            Type = settingName.Type;
            Instance = settingName.Instance;
        }

        [CanBeNull]
        [AutoEqualityProperty]
        public SoftString Namespace { get; set; }

        [CanBeNull]
        [AutoEqualityProperty]
        public SoftString Type { get; set; }

        [NotNull]
        [AutoEqualityProperty]
        public SoftString Property
        {
            get => _property;
            set => _property = value ?? throw new ArgumentNullException(nameof(Property));
        }

        [CanBeNull]
        [AutoEqualityProperty]
        public SoftString Instance { get; set; }

        [ContractAnnotation("value: null => halt"), NotNull]
        public static SettingName Parse([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var match = Regex.Match(value, NamePattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return new SettingName(match.Groups["Property"].Value)
                {
                    Namespace = match.Groups["Namespace"].Value.NullIfEmpty(),
                    Type = match.Groups["Type"].Value.NullIfEmpty(),
                    Instance = match.Groups["Instance"].Value.NullIfEmpty(),
                };
            }

            throw ("SettingNameFormatException", $"Could not parse setting {value.QuoteWith("'")}. Expected format: {Format}").ToDynamicException();
        }

        [SuppressMessage("ReSharper", "RedundantToStringCall")] // it's not redundant, SoftString does not implicitly convert to string.
        public override string ToString()
        {
            return new StringBuilder()
                .AppendWhen(Namespace.IsNotNullOrEmpty(), () => $"{Namespace?.ToString()}{NamespaceSeparator}")
                .AppendWhen(Type.IsNotNullOrEmpty(), () => $"{Type?.ToString()}{TypeSeparator}")
                .Append(Property)
                .AppendWhen(Instance.IsNotNullOrEmpty(), () => $"{InstanceSeparator}{Instance?.ToString()}")
                .ToString();
        }

        public static implicit operator string(SettingName settingName) => settingName?.ToString();

        public static implicit operator SoftString(SettingName settingName) => settingName?.ToString();
    }

    public partial class SettingName : IEquatable<SettingName>
    {
        public bool Equals(SettingName other) => AutoEquality<SettingName>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is SettingName settingName && Equals(settingName);

        public override int GetHashCode() => AutoEquality<SettingName>.Comparer.GetHashCode(this);
    }
}