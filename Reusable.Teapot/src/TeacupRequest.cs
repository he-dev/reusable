using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public class TeacupRequest
    {
        public UriString Uri { get; set; }

        public HttpMethod Method { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public long? ContentLength { get; set; }

        public Stream ContentCopy { get; set; }
    }
}