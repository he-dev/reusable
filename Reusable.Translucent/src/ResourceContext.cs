using System.IO;
using System.Threading.Tasks;

namespace Reusable.Translucent
{
    public class ResourceContext
    {
        public Request Request { get; set; }

        public Response Response { get; set; }
    }

    public delegate Task<Stream> CreateBodyStreamDelegate();
}