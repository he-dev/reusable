using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;
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
            builder.DisplayScalar(x => x.Uri);
            builder.DisplayScalar(x => x.Exists);
            builder.DisplayScalar(x => x.Format);
        });

        #region IResourceInfo

        public virtual IImmutableContainer Properties { get; }

        public UriString Uri => Properties.GetItemOrDefault(ResourceProperty.Uri);

        public bool Exists => Properties.GetItemOrDefault(ResourceProperty.Exists);

        public virtual MimeType Format => Properties.GetItemOrDefault(ResourceProperty.Format);

        public abstract Task CopyToAsync(Stream stream);

        #endregion

        #region IEquatable<IResourceInfo>

        public override bool Equals(object obj) => obj is IResource resource && Equals(resource);

        public bool Equals(IResource other) => ResourceEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => !string.IsNullOrWhiteSpace(other) && ResourceEqualityComparer.Default.Equals((UriString)other, Uri);

        public override int GetHashCode() => ResourceEqualityComparer.Default.GetHashCode(this);

        #endregion

        public virtual void Dispose() { }

        public class DoesNotExist : Resource
        {
            public DoesNotExist([NotNull] IImmutableContainer properties) : base(properties) { }

            public static IResource FromRequest(Request request)
            {
                return new DoesNotExist(request.Context.CopyResourceProperties().SetUri(request.Uri));
            }

            public override Task CopyToAsync(Stream stream)
            {
                throw new InvalidOperationException($"Cannot copy resource '{Uri}' to stream because it does not exist.");
            }
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(Resource))]
    public class ResourceProperty : SelectorBuilder<ResourceProperty>
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

        public static Selector<Func<Stream, Task<object>>> DeserializeAsync { get; } = Select(() => DeserializeAsync);
    }

    public class PlainResource : Resource
    {
        [CanBeNull]
        private readonly string _value;

        public PlainResource(string value, IImmutableContainer properties)
            : base(properties
                .SetExists(!(value is null))
                .SetFormat(MimeType.Plain)
                .SetItem(ResourceProperty.DeserializeAsync, async stream => await ResourceHelper.DeserializeTextAsync(stream, Encoding.UTF8)))
        {
            _value = value;
        }

        public override async Task CopyToAsync(Stream stream)
        {
            using (var s = await ResourceHelper.SerializeTextAsync(_value))
            {
                await s.Rewind().CopyToAsync(stream);
            }
        }
    }

    public class JsonResource : Resource
    {
        [CanBeNull]
        private readonly string _value;

        public JsonResource(string value, IImmutableContainer properties)
            : base(properties
                .SetExists(!(value is null))
                .SetFormat(MimeType.Json)
                .SetItem(ResourceProperty.DeserializeAsync, async stream => await ResourceHelper.DeserializeTextAsync(stream, Encoding.UTF8)))
        {
            _value = value;
        }

        public override async Task CopyToAsync(Stream stream)
        {
            using (var s = await ResourceHelper.SerializeTextAsync(_value))
            {
                await s.Rewind().CopyToAsync(stream);
            }
        }
    }

    public class ObjectResource : Resource
    {
        [CanBeNull]
        private readonly object _value;

        public ObjectResource(object value, IImmutableContainer properties)
            : base(properties
                .SetExists(!(value is null))
                .SetFormat(MimeType.Binary)
                .SetItem(ResourceProperty.DeserializeAsync, async stream => await ResourceHelper.DeserializeBinaryAsync(stream)))
        {
            _value = value;
        }

        public override async Task CopyToAsync(Stream stream)
        {
            using (var s = await ResourceHelper.SerializeBinaryAsync(_value))
            {
                await s.Rewind().CopyToAsync(stream);
            }
        }
    }

    public class StreamResource : Resource
    {
        [CanBeNull]
        private readonly Stream _value;

        internal StreamResource(Stream value, IImmutableContainer properties)
            : base(properties
                .SetExists(value != null && value != Stream.Null)
                .SetFormat(MimeType.Binary))
        {
            _value = value;
        }

        public override async Task CopyToAsync(Stream stream)
        {
            await _value.Rewind().CopyToAsync(stream);
        }
    }
}