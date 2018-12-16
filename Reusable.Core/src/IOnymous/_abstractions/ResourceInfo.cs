using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceInfo : IEquatable<IResourceInfo>, IEquatable<string>
    {
        [NotNull]
        SimpleUri Uri { get; }

        bool Exists { get; }

        long? Length { get; }

        DateTime? CreatedOn { get; }

        DateTime? ModifiedOn { get; }

        Task CopyToAsync(Stream stream);

        Task<object> DeserializeAsync(Type targetType);
    }

    [PublicAPI]
    public abstract class ResourceInfo : IResourceInfo
    {
        protected const int OutOfRange = -1;

        protected ResourceInfo([NotNull] SimpleUri uri) => Uri = uri ?? throw new ArgumentNullException(nameof(uri));

        #region IValueInfo

        public virtual SimpleUri Uri { get; }

        public abstract bool Exists { get; }

        public abstract long? Length { get; }

        public abstract DateTime? CreatedOn { get; }

        public abstract DateTime? ModifiedOn { get; }

        public abstract Task CopyToAsync(Stream stream);

        public abstract Task<object> DeserializeAsync(Type targetType);

        #endregion

        #region IEquatable<IFileInfo>

        public override bool Equals(object obj) => obj is IResourceInfo file && Equals(file);

        public bool Equals(IResourceInfo other) => ResourceInfoEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceInfoEqualityComparer.Default.Equals(other, Uri);

        public override int GetHashCode() => ResourceInfoEqualityComparer.Default.GetHashCode(this);

        #endregion
    }

    public static class ResourceInfoExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this IResourceInfo resourceInfo) => (T)(await resourceInfo.DeserializeAsync(typeof(T)));
    }

    public class ResourceInfoEqualityComparer : IEqualityComparer<IResourceInfo>, IEqualityComparer<Uri>, IEqualityComparer<string>
    {
        private static readonly IEqualityComparer ResourceUriComparer = StringComparer.OrdinalIgnoreCase;

        [NotNull]
        public static ResourceInfoEqualityComparer Default { get; } = new ResourceInfoEqualityComparer();

        public bool Equals(IResourceInfo x, IResourceInfo y) => Equals(x?.Uri, y?.Uri);

        public int GetHashCode(IResourceInfo obj) => GetHashCode(obj.Uri);

        public bool Equals(Uri x, Uri y) => ResourceUriComparer.Equals(x, y);

        public int GetHashCode(Uri obj) => ResourceUriComparer.GetHashCode(obj);

        public bool Equals(string x, string y) => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrWhiteSpace(y) && Equals(new Uri(x), new Uri(y));

        public int GetHashCode(string obj) => GetHashCode(new Uri(obj));
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