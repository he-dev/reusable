using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.SmartConfig.Data
{
    [PublicAPI]
    public interface ISetting : IEquatable<ISetting>, IEquatable<SoftString>, IEquatable<string>
    {
        [NotNull]
        SoftString Name { get; }

        [CanBeNull]
        object Value { get; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Setting : ISetting
    {
        private static readonly IEqualityComparer<SoftString> Comparer = RelayEqualityComparer<SoftString>.Create((left, right) => left.Equals(right), obj => obj.GetHashCode());

        [DebuggerStepThrough]
        public Setting([NotNull] SoftString name, object value) : this(name) => Value = value ;//?? throw new ArgumentNullException(nameof(value));

        [DebuggerStepThrough]
        public Setting([NotNull] SoftString name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        public SoftString Name { [DebuggerStepThrough]get; }

        public object Value { [DebuggerStepThrough]get; }

        public static ISetting Create(SoftString name, object value) => new Setting(name, value);

        public static ISetting Create(SoftString name) => new Setting(name);

        private string DebuggerDisplay => ToString();

        public override string ToString() => Name.ToString();

        public static implicit operator string(Setting setting) => setting.ToString();

        #region IEquatable

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj) => (obj is ISetting s && Equals(s)) || (obj is SoftString ss && Equals(ss)) || (obj is string str && Equals(str));

        public bool Equals(ISetting obj) => Comparer.Equals(Name, obj?.Name);

        public bool Equals(SoftString obj) => Comparer.Equals(Name, obj);

        public bool Equals(string obj) => Comparer.Equals(Name, obj);

        #endregion
    }
}