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
    public class Request
    {
        [NotNull]
        public UriString Uri { get; set; } // = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        [NotNull]
        public RequestMethod Method { get; set; } = RequestMethod.None;

        [NotNull]
        public IImmutableContainer Metadata { get; set; } = ImmutableContainer.Empty;

        [CanBeNull]
        public object Body { get; set; }

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

        public static readonly Selector<Encoding> Encoding = This.Select(() => Encoding);
        
        public static readonly Selector<CancellationToken> CancellationToken = This.Select(() => CancellationToken);

        #endregion
    }
}