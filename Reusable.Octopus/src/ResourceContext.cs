using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reusable.Octopus.Data;

namespace Reusable.Octopus;

public class ResourceContext
{
    private Response _response = default!;
    private readonly Request _request = default!;

    public Request Request
    {
        get => _request;
        init
        {
            _request = value;
            _request.Items["Log"] = new List<string>();
        }
    }

    public Response Response
    {
        get => _response;
        set
        {
            _response = value;

            // Use the same instance for both so we don't have to merge it later.
            _response.Items["Log"] = _request.Items["Log"];
        }
    }
}

public delegate Task<Stream> BodyStreamFunc();