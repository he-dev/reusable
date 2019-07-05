using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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
        protected static class Validations
        {
            public static readonly IImmutableList<IValidationRule<IResource, object>> Exists =
                ValidationRuleCollection
                    .For<IResource>()
                    .Accept(b => b.When(x => x.Exists).Message((x, _) => $"Resource '{x.Uri}' does not exist."));
        }

        public static readonly From<IResourceProperties> PropertySelector = From<IResourceProperties>.This;

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

        public UriString Uri => Properties.GetItemOrDefault(PropertySelector.Select(x => x.Uri));

        public bool Exists => Properties.GetItemOrDefault(PropertySelector.Select(x => x.Exists));

        public virtual MimeType Format => Properties.GetItemOrDefault(PropertySelector.Select(x => x.Format));

        public abstract Task CopyToAsync(Stream stream);

        #endregion

        #region IEquatable<IResourceInfo>

        public override bool Equals(object obj) => obj is IResource resource && Equals(resource);

        public bool Equals(IResource other) => ResourceEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceEqualityComparer.Default.Equals((UriString)other, Uri);

        public override int GetHashCode() => ResourceEqualityComparer.Default.GetHashCode(this);

        #endregion

        public virtual void Dispose() { }
    }
}