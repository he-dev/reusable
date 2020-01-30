using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public class ResourceContext
    {
        public Request Request { get; set; } = default!;

        public Response Response { get; set; } = default!;
    }

    public delegate Task<Stream> CreateBodyStreamDelegate();
}