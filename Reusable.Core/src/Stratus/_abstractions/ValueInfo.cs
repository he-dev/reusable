using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Stratus
{
    [PublicAPI]
    public interface IValueInfo : IEquatable<IValueInfo>, IEquatable<string>
    {
        [NotNull]
        string Name { get; }

        bool Exists { get; }

        long? Length { get; }

        DateTime? CreatedOn { get; }

        DateTime? ModifiedOn { get; }

        Task CopyToAsync(Stream stream);

        Task<object> DeserializeAsync(Type targetType);
    }

    [PublicAPI]
    public abstract class ValueInfo : IValueInfo
    {
        protected const int OutOfRange = -1;

        protected ValueInfo([NotNull] string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        #region IValueInfo

        public virtual string Name { get; }

        public abstract bool Exists { get; }

        public abstract long? Length { get; }

        public abstract DateTime? CreatedOn { get; }

        public abstract DateTime? ModifiedOn { get; }

        public abstract Task CopyToAsync(Stream stream);

        public abstract Task<object> DeserializeAsync(Type targetType);

        #endregion

        #region IEquatable<IFileInfo>

        public override bool Equals(object obj) => obj is IValueInfo file && Equals(file);

        public bool Equals(IValueInfo other) => ValueInfoEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => ValueInfoEqualityComparer.Default.Equals(other, Name);

        public override int GetHashCode() => ValueInfoEqualityComparer.Default.GetHashCode(this);

        #endregion
    }

    public static class ValueInfoExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this IValueInfo valueInfo) => (T)(await valueInfo.DeserializeAsync(typeof(T)));
    }

    public class ValueInfoEqualityComparer : IEqualityComparer<IValueInfo>, IEqualityComparer<string>
    {
        private static readonly IEqualityComparer PathComparer = StringComparer.OrdinalIgnoreCase;

        [NotNull]
        public static ValueInfoEqualityComparer Default { get; } = new ValueInfoEqualityComparer();

        public bool Equals(IValueInfo x, IValueInfo y) => Equals(x?.Name, y?.Name);

        public int GetHashCode(IValueInfo obj) => GetHashCode(obj.Name);

        public bool Equals(string x, string y) => PathComparer.Equals(x, y);

        public int GetHashCode(string obj) => PathComparer.GetHashCode(obj);
    }

    //public readonly struct ValueInfoType : IEquatable<ValueInfoType>
    //{
    //    private ValueInfoType([NotNull] string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

    //    [AutoEqualityProperty]
    //    public string Name { get; }

    //    public static ValueInfoType Create(string name) => new ValueInfoType(name);

    //    public override bool Equals(object obj) => obj is ValueInfoType type && Equals(type);

    //    public bool Equals(ValueInfoType other) => AutoEquality<ValueInfoType>.Comparer.Equals(this, other);

    //    public override int GetHashCode() => AutoEquality<ValueInfoType>.Comparer.GetHashCode(this);

    //    public static implicit operator ValueInfoType(string name) => new ValueInfoType(name);

    //    public static implicit operator string(ValueInfoType type) => type.Name;
    //}
}