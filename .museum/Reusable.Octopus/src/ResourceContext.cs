using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus;

public class ResourceContext
{
    // Use the same instance for both (Request & Response) so it doesn't have to be merged later.
    private List<string> _log = new();

    private Response _response = default!;

    public ResourceContext(Request request)
    {
        if (request.ResourceName.Any() == false) throw DynamicException.Create(request.GetType().ToPrettyString(), $"Resource name must not be null nor empty.");
        if (request.Method == RequestMethod.None) throw DynamicException.Create(request.GetType().ToPrettyString(), $"Request method must not be '{nameof(RequestMethod.None)}'.");

        Request = request;
        Request.Items["Log"] = _log;
    }

    public Request Request { get; }

    public Response Response
    {
        get => _response;
        set
        {
            _response = value;
            _response.Items["Log"] = Request.Items["Log"];
        }
    }
}

public delegate Task<Stream> CreateStreamAsync();