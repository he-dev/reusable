using System;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class Request : IDisposable
    {
        [NotNull]
        public UriString Uri { get; set; } // = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        [NotNull]
        public Option<RequestMethod> Method { get; set; } = RequestMethod.None;

        [NotNull]
        public IImmutableContainer Metadata { get; set; } = ImmutableContainer.Empty;

        [CanBeNull]
        public object Body { get; set; }

        public void Dispose()
        {
            (Body as IDisposable)?.Dispose();
        }

        #region Methods

        public class Get : Request
        {
            public Get(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Get;
            }
        }

        public class Post : Request
        {
            public Post(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Post;
            }
        }

        public class Put : Request
        {
            public Put(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Put;
            }
        }

        public class Delete : Request
        {
            public Delete(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Delete;
            }
        }

        #endregion

        #region Properties

        private static readonly From<Request> This;

        //public static readonly Selector<MimeType> Accept = This.Select(() => Accept);
        
        public static readonly Selector<bool> IsOptional = This.Select(() => IsOptional);

        //public static readonly Selector<TimeSpan> CacheTimeout = This.Select(() => CacheTimeout);
        
        //public static readonly Selector<bool> IsExternallyOwned = This.Select(() => IsExternallyOwned);

        public static readonly Selector<Encoding> Encoding = This.Select(() => Encoding);
        
        public static readonly Selector<CancellationToken> CancellationToken = This.Select(() => CancellationToken);

        #endregion
    }
}