using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Flawless;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResource : IDisposable, IEquatable<IResource>, IEquatable<string>
    {
        [NotNull]
        IImmutableContainer Properties { get; }

        [NotNull]
        UriString Uri { get; }

        bool Exists { get; }

        [NotNull]
        MimeType Format { get; }

        Task CopyToAsync(Stream stream);
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Resource : IResource
    {
        protected Resource([NotNull] IImmutableContainer properties)
        {
            // todo - why do I need this?
            //Uri = uri.IsRelative ? new UriString($"{ResourceSchemes.IOnymous}:{uri}") : uri;
            Properties = properties;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Uri);
            builder.DisplayValue(x => x.Exists);
            builder.DisplayValue(x => x.Format);
        });

        #region IResourceInfo

        public virtual IImmutableContainer Properties { get; }

        public UriString Uri => Properties.GetItemOrDefault(Property.Uri);

        public bool Exists => Properties.GetItemOrDefault(Property.Exists);

        public virtual MimeType Format => Properties.GetItemOrDefault(Property.Format);

        public abstract Task CopyToAsync(Stream stream);

        #endregion

        #region IEquatable<IResourceInfo>

        public override bool Equals(object obj) => obj is IResource resource && Equals(resource);

        public bool Equals(IResource other) => ResourceEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceEqualityComparer.Default.Equals((UriString)other, Uri);

        public override int GetHashCode() => ResourceEqualityComparer.Default.GetHashCode(this);

        #endregion

        public virtual void Dispose() { }

        [UseType, UseMember]
        [PlainSelectorFormatter]
        [Rename(nameof(Resource))]
        public class Property : SelectorBuilder<Property>
        {
            public static readonly Selector<UriString> Uri = Select(() => Uri);

            public static readonly Selector<bool> Exists = Select(() => Exists);

            public static readonly Selector<long> Length = Select(() => Length);

            public static readonly Selector<DateTime> CreateOn = Select(() => CreateOn);

            public static readonly Selector<DateTime> ModifiedOn = Select(() => ModifiedOn);

            public static readonly Selector<MimeType> Format = Select(() => Format);

            public static readonly Selector<Type> DataType = Select(() => DataType);

            public static readonly Selector<Encoding> Encoding = Select(() => Encoding);

            public static readonly Selector<string> ActualName = Select(() => ActualName);
        }
    }
}