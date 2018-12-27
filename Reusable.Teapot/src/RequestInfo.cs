using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public class RequestInfo : IDisposable
    {
        public UriString Uri { get; set; }

        public SoftString Method { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public long? ContentLength { get; set; }

        public MemoryStream ContentCopy { get; set; }

        public void Dispose()
        {
            ContentCopy?.Dispose();
        }
    }
}