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
    public class Response : IDisposable
    {
        public ResourceStatusCode StatusCode { get; set; }
        
        public Stream Body { get; set; }

        public MimeType ContentType { get; set; }

        public IImmutableContainer Metadata { get; set; }

        public void Dispose() => Body?.Dispose();

        public class OK : Response
        {
            public OK()
            {
                StatusCode = ResourceStatusCode.OK;
            }
        }
        
        public class NotFound : Response
        {
            public NotFound()
            {
                StatusCode = ResourceStatusCode.NotFound;
            }
        }
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

        public static readonly Selector<DateTime> CreateOn = Select(() => CreateOn);

        public static readonly Selector<DateTime> ModifiedOn = Select(() => ModifiedOn);

        //public static readonly Selector<MimeType> Format = Select(() => Format);

        public static readonly Selector<Type> DataType = Select(() => DataType);

        public static readonly Selector<Encoding> Encoding = Select(() => Encoding);

        public static readonly Selector<string> ActualName = Select(() => ActualName);

        //public static readonly Selector<Func<Stream, Task<object>>> DeserializeAsync = Select(() => DeserializeAsync);
    }
}