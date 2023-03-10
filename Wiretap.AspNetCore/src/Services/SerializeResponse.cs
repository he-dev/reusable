using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reusable.Extensions;
using Reusable.Wiretap.AspNetCore.Abstractions;

namespace Reusable.Wiretap.AspNetCore.Services;

public class SerializeResponse : ISerialize<HttpResponse>
{
    public async Task<string?> Invoke(HttpResponse response)
    {
        if (response.ContentLength > 0 && response.Body.TryRewind())
        {
            try
            {
                using var reader = new StreamReader(response.Body);
                return await reader.ReadToEndAsync();
            }
            finally
            {
                response.Body.Rewind();
            }
        }

        return default;
    }
}