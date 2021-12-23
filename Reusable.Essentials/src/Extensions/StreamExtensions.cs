using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Essentials.Extensions;

public static class StreamExtensions
{
    public static T Rewind<T>(this T stream) where T : Stream
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return stream;
    }

    public static async Task<string> ReadTextAsync(this Stream stream, Encoding? encoding = default)
    {
        using var streamReader = new StreamReader(stream.Rewind(), encoding ?? Encoding.UTF8);
        return await streamReader.ReadToEndAsync();
    }
    
    public static string ReadText(this Stream stream, Encoding? encoding = default)
    {
        using var streamReader = new StreamReader(stream.Rewind(), encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}