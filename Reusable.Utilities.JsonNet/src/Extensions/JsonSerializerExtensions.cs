using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Extensions
{
    public static class JsonSerializerExtensions
    {
        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

        private static readonly int BufferSize = 1024;

        public static void Serialize<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] Stream stream, [NotNull] T obj)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            using (var textWriter = new StreamWriter(stream, UTF8NoBOM, BufferSize, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonSerializer.Serialize(jsonWriter, obj);
                jsonWriter.Flush();
            }
        }

        public static Stream Serialize<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] T obj)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var output = new MemoryStream();
            using (var textWriter = new StreamWriter(output, UTF8NoBOM, BufferSize, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonSerializer.Serialize(jsonWriter, obj);
                jsonWriter.Flush();
            }

            return output;
        }

        [NotNull]
        public static T Deserialize<T>([NotNull] this JsonSerializer jsonSerializer, [NotNull] Stream stream)
        {
            if (jsonSerializer == null) throw new ArgumentNullException(nameof(jsonSerializer));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using (var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, BufferSize, leaveOpen: true))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}