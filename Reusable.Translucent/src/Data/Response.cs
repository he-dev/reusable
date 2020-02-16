using System;
using System.IO;

namespace Reusable.Translucent.Data
{
    public class Response : IDisposable
    {
        public string ResourceName { get; set; }
        
        public ResourceStatusCode StatusCode { get; set; }

        public object? Body { get; set; }

        /// <summary>
        /// True if Body was retrieved from cache.
        /// </summary>
        public bool Cached { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public static Response Success() => new Response { StatusCode = ResourceStatusCode.Success };
        
        public static Response NotFound(string resourceName) => new Response { ResourceName = resourceName, StatusCode = ResourceStatusCode.NotFound };

        public void Dispose()
        {
            if (Body is Stream stream && !Cached)
            {
                stream.Dispose();
            }
        }
    }
}