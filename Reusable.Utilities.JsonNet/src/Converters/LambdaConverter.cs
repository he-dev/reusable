using System;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Converters
{
    public delegate object WriteJsonCallback<in T>(T value);
    
    public class LambdaConverter<T> : JsonConverter<T>
    {
        private readonly WriteJsonCallback<T> _writeJsonCallback;
        
        public LambdaConverter(WriteJsonCallback<T> writeJsonCallback)
        {
            _writeJsonCallback = writeJsonCallback;
        }
        
        public static LambdaConverter<T> Create(WriteJsonCallback<T> convert) => new LambdaConverter<T>(convert);

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {            
            serializer.Serialize(writer, _writeJsonCallback(value));
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}