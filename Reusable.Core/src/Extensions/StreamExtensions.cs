using System.IO;

namespace Reusable.Extensions
{
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
    }
}