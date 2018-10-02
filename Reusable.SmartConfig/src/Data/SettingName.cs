using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.SmartConfig.Data
{   
    [PublicAPI]
    public class SettingName
    {
        public const string NamespaceSeparator = "+";
        public const string TypeSeparator = ".";
        public const string InstanceSeparator = ",";

        // [Namespace+][Type.]Member[,Instance]
        public static readonly string Format =
            $"[{nameof(Namespace)}{NamespaceSeparator}]" +
            $"[{nameof(Type)}{TypeSeparator}]" +
            $"{nameof(Member)}" +
            $"[{InstanceSeparator}{nameof(Instance)}]";

        private static readonly string NamePattern =
            $"(?:(?<Namespace>[a-z0-9_.]+)\\{NamespaceSeparator})?" +
            $"(?:(?<Type>[a-z0-9_]+)\\{TypeSeparator})?" +
            $"(?:(?<Member>[a-z0-9_]+))" +
            $"(?:{InstanceSeparator}(?<Instance>[a-z0-9_]+))?";

        [NotNull] private SoftString _member;

        public SettingName([NotNull] SoftString member)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public SettingName(SettingName settingName) : this(settingName.Member)
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
        public SoftString Member
        {
            get => _member;
            set => _member = value ?? throw new ArgumentNullException(nameof(Member));
        }

        [CanBeNull]
        [AutoEqualityProperty]
        public SoftString Instance { get; set; }

        [ContractAnnotation("value: null => halt"), NotNull]
        public static SettingName Parse([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var match = Regex.Match(value, NamePattern, RegexOptions.IgnoreCase);
            return
                match.Success
                    ? new SettingName(match.Groups[nameof(Member)].Value)
                    {
                        Namespace = match.Groups[nameof(Namespace)].Value.NullIfEmpty(),
                        Type = match.Groups[nameof(Type)].Value.NullIfEmpty(),
                        Instance = match.Groups[nameof(Instance)].Value.NullIfEmpty(),
                    }
                    : throw ("SettingNameFormat", $"Could not parse setting {value.QuoteWith("'")}. Expected format: {Format}").ToDynamicException();
        }

        [SuppressMessage("ReSharper", "RedundantToStringCall")]
        // it's not redundant, SoftString does not implicitly convert to string.
        public override string ToString()
        {
            return new StringBuilder()
                .AppendWhen(Namespace.IsNotNullOrEmpty(), () => $"{Namespace?.ToString()}{NamespaceSeparator}")
                .AppendWhen(Type.IsNotNullOrEmpty(), () => $"{Type?.ToString()}{TypeSeparator}")
                .Append(Member)
                .AppendWhen(Instance.IsNotNullOrEmpty(), () => $"{InstanceSeparator}{Instance?.ToString()}")
                .ToString();
        }

        public static implicit operator SettingName(string settingName) => Parse(settingName);

        public static implicit operator string(SettingName settingName) => settingName?.ToString();

        public static implicit operator SoftString(SettingName settingName) => settingName?.ToString();

        #region IEquatable<SettingName>

        public bool Equals(SettingName other) => AutoEquality<SettingName>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is SettingName settingName && Equals(settingName);

        public override int GetHashCode() => AutoEquality<SettingName>.Comparer.GetHashCode(this);

        #endregion
    }
}