using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reusable.Marbles.Extensions;

namespace Reusable.Wiretap.Utilities.AspNetCore.Services;

public class SerializeRequestBody
{
    public virtual async Task<string?> Invoke(HttpContext context)
    {
        if (context.Request.ContentLength > 0)
        {
            try
            {
                await using var requestCopy = new MemoryStream();
                using var requestReader = new StreamReader(requestCopy);
                context.Request.EnableBuffering();
                await context.Request.Body.CopyToAsync(requestCopy);
                requestCopy.Rewind();
                return await requestReader.ReadToEndAsync();
            }
            finally
            {
                context.Request.Body.Rewind();
            }
        }
        else
        {
            return default;
        }
    }
}