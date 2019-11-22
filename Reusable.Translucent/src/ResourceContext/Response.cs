using System;
using System.Collections.Generic;
using System.IO;

namespace Reusable.Translucent
{
    public class Response : IDisposable
    {
        public ResourceStatusCode StatusCode { get; set; }

        public object? Body { get; set; }

        /// <summary>
        /// True if Body was retrieved from cache.
        /// </summary>
        public bool Cached { get; set; }

        /// <summary>
        /// Gets or sets the controllers that handled this request. In case of a GET request there might be more than one. In case of a success the last controller is the owner of this response.
        /// </summary>
        public List<object> HandledBy { get; } = new List<object>();

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