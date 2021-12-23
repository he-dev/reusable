using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Extensions;

public static class JsonSerializerExtensions
{
    private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

    private static readonly int BufferSize = 1024;

    public static void Serialize<T>(this JsonSerializer jsonSerializer, Stream stream, [DisallowNull] T obj)
    {
        using (var textWriter = new StreamWriter(stream, UTF8NoBOM, BufferSize, leaveOpen: true))
        using (var jsonWriter = new JsonTextWriter(textWriter))
        {
            jsonSerializer.Serialize(jsonWriter, obj);
            jsonWriter.Flush();
        }
    }

    public static Stream Serialize<T>(this JsonSerializer jsonSerializer, [DisallowNull] T obj)
    {
        var output = new MemoryStream();
        using (var textWriter = new StreamWriter(output, UTF8NoBOM, BufferSize, leaveOpen: true))
        using (var jsonWriter = new JsonTextWriter(textWriter))
        {
            jsonSerializer.Serialize(jsonWriter, obj);
            jsonWriter.Flush();
        }

        return output;
    }

    [return: NotNull]
    public static T Deserialize<T>(this JsonSerializer jsonSerializer, Stream stream)
    {
        using (var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, BufferSize, leaveOpen: true))
        using (var jsonTextReader = new JsonTextReader(streamReader))
        {
            return jsonSerializer.Deserialize<T>(jsonTextReader);
        }
    }
    
    public static object? Deserialize(this JsonSerializer jsonSerializer, Stream stream, Type toType)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, BufferSize, leaveOpen: true);
        using var jsonTextReader = new JsonTextReader(streamReader);
        return jsonSerializer.Deserialize(jsonTextReader, toType);
    }
}