using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Essentials;

namespace Reusable.Utilities.JsonNet;

[PublicAPI]
public static class PrettyJsonHelper
{
    /// <summary>
    /// Casts JsonReader to JTokenReader.
    /// </summary>
    public static JTokenReader TokenReader(this JsonReader reader) => (JTokenReader)reader;

    /// <summary>
    /// Gets the value from the '$type' property of the current-token.
    /// </summary>
    public static Type GetJsonType(this JsonReader reader)
    {
        var typeName =
            reader
                .TokenReader()
                .CurrentToken
                .SelectToken("$.$type")
                .Value<string>() ?? throw DynamicException.Create("TypePropertyNotFound", $"Could not find '$type' property on '{reader.TokenReader().CurrentToken}'.");

        return Type.GetType(typeName) ?? throw DynamicException.Create("TypeNotFound", $"Could not find type '{typeName}'.");
    }

    /// <summary>
    /// Deserializes an object of the specified type.
    /// </summary>
    public static T ToObject<T>(this JsonReader reader, Type type, JsonSerializer serializer)
    {
        return (T)reader.TokenReader().CurrentToken.ToObject(type, serializer);
    }
}