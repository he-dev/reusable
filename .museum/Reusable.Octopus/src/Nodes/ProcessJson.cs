using System.IO;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Octopus.Nodes;

using static RequestMethod;
using static ResourceStatusCode;

[PublicAPI]
public class ProcessJson : ResourceNode
{
    private JsonSerializer? _responseSerializer;

    public JsonSerializer RequestSerializer { get; set; } = new()
    {
        Converters =
        {
            new SoftStringConverter(),
            new StringEnumConverter(),
            new ColorConverter(),
        }
    };

    public JsonSerializer ResponseSerializer
    {
        get => _responseSerializer ?? RequestSerializer;
        set => _responseSerializer = value;
    }

    public override async Task InvokeAsync(ResourceContext context)
    {
        if (context.Request is { } request)
        {
            if (request.Method.In(Create, Update))
            {
                var serializable =
                    request.Data.Value is not string &&
                    request.Data.Value is not Stream &&
                    request.Data.Value is not ManageString;

                if (serializable)
                {
                    context.Request.Data.Push(RequestSerializer.Serialize(request.Data.Value));
                }
            }

            await InvokeNext(context);

            if (request.Method.In(Read) && context.Response.StatusCode == Success)
            {
                if (context.Response.Body.Value is Stream stream)
                {
                    //if (ResponseSerializer.Deserialize(stream, request.ToType) is { } result)
                    {
                        //context.Response.Body.Push(result);
                    }
                }
            }
        }
        else
        {
            await InvokeNext(context);
        }

        await InvokeNext(context);
    }
}

public record struct Convertible<T>(object Value) where T : ResourceNode;