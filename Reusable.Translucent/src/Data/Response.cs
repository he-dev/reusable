using System;
using System.IO;

namespace Reusable.Translucent.Data
{
    public class Response : IDisposable
    {
        public ResourceStatusCode StatusCode { get; set; }

        public object? Body { get; set; }

        /// <summary>
        /// True if Body was retrieved from cache.
        /// </summary>
        public bool Cached { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public static Response OK() => new Response { StatusCode = ResourceStatusCode.OK };
        
        public static Response NotFound() => new Response { StatusCode = ResourceStatusCode.NotFound };

        public void Dispose()
        {
            if (Body is Stream stream && !Cached)
            {
                stream.Dispose();
            }
        }
    }

    public enum ResourceStatusCode
    {
        // ReSharper disable once InconsistentNaming
        OK,
        NotFound
    }
}