using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reusable.Extensions;
using Reusable.Wiretap.AspNetCore.Abstractions;

namespace Reusable.Wiretap.AspNetCore.Services;

public class SerializeRequest : ISerialize<HttpRequest>
{
    public async Task<string?> Invoke(HttpRequest request)
    {
        request.EnableBuffering();
        if (request.ContentLength > 0 && request.Body.TryRewind())
        {
            try
            {
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                return await reader.ReadToEndAsync();
            }
            finally
            {
                request.Body.Rewind();
            }
        }

        return default;
    }
}