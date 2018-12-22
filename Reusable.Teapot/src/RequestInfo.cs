using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Reusable.Teapot
{
    public class RequestInfo : IDisposable
    {
        public long? ContentLength { get; set; }

        //public PathString Path { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public MemoryStream ContentCopy { get; set; }
        
        public void Dispose()
        {
            ContentCopy?.Dispose();
        }
    }
}