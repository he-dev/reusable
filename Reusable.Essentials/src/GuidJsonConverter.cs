using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reusable.Essentials;

public class GuidJsonConverter : JsonConverter<Guid>
{
    public GuidJsonConverter(string format) => Format = format;

    public string Format { get; }

    /// <summary>
    /// 32 digits:
    /// 00000000000000000000000000000000
    /// </summary>
    public static readonly GuidJsonConverter N = new(nameof(N));
    
    /// <summary>
    /// 32 digits separated by hyphens:
    /// 00000000-0000-0000-0000-000000000000
    /// </summary>
    public static readonly GuidJsonConverter D = new(nameof(D));
    
    /// <summary>
    /// 32 digits separated by hyphens, enclosed in braces:
    /// {00000000-0000-0000-0000-000000000000}
    /// </summary>
    public static readonly GuidJsonConverter B = new(nameof(B));
    
    /// <summary>
    /// 32 digits separated by hyphens, enclosed in parentheses:
    /// (00000000-0000-0000-0000-000000000000)
    /// </summary>
    public static readonly GuidJsonConverter P = new(nameof(P));
    
    /// <summary>
    /// Four hexadecimal values enclosed in braces, where the fourth value is a subset of eight hexadecimal values that is also enclosed in braces:
    /// {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
    /// </summary>
    public static readonly GuidJsonConverter X = new(nameof(X));

    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Guid.ParseExact(reader.GetString()!, Format);
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}