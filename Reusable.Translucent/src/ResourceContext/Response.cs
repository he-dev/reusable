using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class Response : IDisposable
    {
        public ResourceStatusCode StatusCode { get; set; }

        public object Body { get; set; }

        public IImmutableContainer Metadata { get; set; }

        public static Response OK() => new Response { StatusCode = ResourceStatusCode.OK };
        
        public static Response NotFound() => new Response { StatusCode = ResourceStatusCode.NotFound };

        public void Dispose()
        {
            if (Body is Stream stream && !(Metadata is null) && Metadata.GetItemOrDefault(Request.IsExternallyOwned))
            {
                stream.Dispose();
            }
        }

//        public class OK : Response
//        {
//            public OK()
//            {
//                StatusCode = ResourceStatusCode.OK;
//            }
//        }
//
//        public class NotFound : Response
//        {
//            public NotFound()
//            {
//                StatusCode = ResourceStatusCode.NotFound;
//            }
//        }

        #region Properties

        private static readonly From<Response> This;

        public static readonly Selector<DateTime> CreateOn = This.Select(() => CreateOn);

        public static readonly Selector<DateTime> ModifiedOn = This.Select(() => ModifiedOn);

        public static readonly Selector<string> ActualName = This.Select(() => ActualName);

        #endregion
    }

    public enum ResourceStatusCode
    {
        OK,
        NotFound
    }

    [UseType("Resource"), UseMember]
    [PlainSelectorFormatter]
    public abstract class ResourceProperties : SelectorBuilder<ResourceProperties>
    {
        //public static readonly Selector<UriString> Uri = Select(() => Uri);

        //public static readonly Selector<bool> Exists = Select(() => Exists);

        //public static readonly Selector<long> Length = Select(() => Length);


        //public static readonly Selector<MimeType> Format = Select(() => Format);

        //public static readonly Selector<Type> DataType = Select(() => DataType);

        //


        //public static readonly Selector<Func<Stream, Task<object>>> DeserializeAsync = Select(() => DeserializeAsync);
    }
}