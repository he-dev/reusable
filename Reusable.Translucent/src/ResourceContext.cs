using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public class ResourceContext
    {
        public Request Request { get; set; } = default!;

        public Response? Response { get; set; }

        public bool Processed => Response is {};

        public List<object> Log { get; } = new List<object>();
    }

    public delegate Task<Stream> CreateBodyStreamDelegate();
}