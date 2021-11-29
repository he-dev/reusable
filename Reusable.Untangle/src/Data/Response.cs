using System;
using System.Collections.Generic;
using System.IO;

namespace Reusable.Translucent.Data
{
    public class Response : IDisposable
    {
        public string ResourceName { get; set; } = default!;
        
        public ResourceStatusCode StatusCode { get; set; }

        public object? Body { get; set; }

        public bool ExternallyOwned { get; set; }

        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

        public List<string> Log { get; set; } = default!;

        // ReSharper disable once InconsistentNaming
        public static Response Success() => new Response { StatusCode = ResourceStatusCode.Success };
        
        public static Response NotFound(string resourceName) => new Response { ResourceName = resourceName, StatusCode = ResourceStatusCode.NotFound };

        public void Dispose()
        {
            if (Body is Stream stream && !ExternallyOwned)
            {
                stream.Dispose();
            }
        }
    }
}