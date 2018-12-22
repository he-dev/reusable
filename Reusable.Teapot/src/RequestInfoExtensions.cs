using System.IO;
using Newtonsoft.Json.Linq;

namespace Reusable.Teapot
{
    public static class RequestInfoExtensions
    {
        public static JToken ToJToken(this RequestInfo info)
        {
            if (info.ContentLength == 0)
            {
                // This supports the null-pattern.
                return JToken.Parse("{}");
            }

            using (var memory = new MemoryStream())
            {
                // It needs to be copied because otherwise it'll get disposed.
                info.ContentCopy.Seek(0, SeekOrigin.Begin);
                info.ContentCopy.CopyTo(memory);

                // Rewind to read from the beginning.
                memory.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memory))
                {
                    var body = reader.ReadToEnd();
                    return JToken.Parse(body);
                }
            }
        }
    }
}