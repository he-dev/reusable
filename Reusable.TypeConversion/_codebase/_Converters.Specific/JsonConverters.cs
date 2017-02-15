using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Reusable.TypeConversion
{
    public abstract class JsonConverter<TValue, TResult> : TypeConverter<TValue, TResult>
    {
        protected JsonConverter(JsonSerializerSettings settings)
        {
            Settings = settings;
        }

        protected JsonSerializerSettings Settings { get; }
    }

    public class JsonToObjectConverter<T> : JsonConverter<String, T>
    {
        public JsonToObjectConverter(JsonSerializerSettings settings) : base(settings)
        {
            // there's nothing else to do
        }

        public JsonToObjectConverter() : this(new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            TypeNameHandling = TypeNameHandling.Auto
        })
        {
            // there's nothing else to do
        }

        protected override T ConvertCore(IConversionContext<string> context)
        {
            return JsonConvert.DeserializeObject<T>(context.Value, Settings);
        }        
    }

    public class ObjectToJsonConverter<T> : JsonConverter<T, String>
    {
        public ObjectToJsonConverter(JsonSerializerSettings settings) : base(settings)
        {
            // there's nothing else to do
        }

        public ObjectToJsonConverter() : this(new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            Formatting = Formatting.Indented
        })
        {
            // there's nothing else to do
        }

        protected override string ConvertCore(IConversionContext<T> context)
        {
            return JsonConvert.SerializeObject(context.Value, Settings);
        }
    }
}
