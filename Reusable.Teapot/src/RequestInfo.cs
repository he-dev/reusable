using System.IO;
using Microsoft.AspNetCore.Http;

namespace Reusable.Teapot
{
    public class RequestInfo
    {
        public long? ContentLength { get; set; }

        public PathString Path { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public MemoryStream BodyStreamCopy { get; set; }
    }
}