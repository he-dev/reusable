using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Converters
{
    [PublicAPI]
    public abstract class JsonConverter<TValue, TResult> : TypeConverter<TValue, TResult>
    {
        //[NotNull]
        //private ISet<Type> _stringTypes;

        protected JsonConverter(JsonSerializerSettings settings, ISet<Type> stringTypes)
        {
            Settings = settings;
            StringTypes = stringTypes;
        }

        protected JsonConverter(JsonSerializerSettings settings):this(settings, new HashSet<Type>())
        {
        }

        [NotNull, ItemNotNull]
        protected ISet<Type> StringTypes { get; }
        //{
        //    get => _stringTypes;
        //    set => _stringTypes = value ?? throw new ArgumentNullException(nameof(StringTypes));
        //}

        protected JsonSerializerSettings Settings { get; }

        protected static bool IsQuoted(string text)
        {
            return text.StartsWith("\"") && text.EndsWith("\"");
        }

        protected bool IsStringType(Type type)
        {
            return StringTypes.Contains(type.IsEnum ? typeof(Enum) : type);
        }
    }

    public class JsonToObjectConverter<T> : JsonConverter<String, T>
    {
        public JsonToObjectConverter(JsonSerializerSettings settings, ISet<Type> stringTypes) : base(settings, stringTypes)
        {
            // there's nothing else to do
        }
        
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
            // String-types require quotes for deserialization.
            var requiresQuotes = IsStringType(typeof(T)) && !IsQuoted(context.Value);

            return JsonConvert.DeserializeObject<T>(requiresQuotes ? Quote(context.Value) : context.Value, Settings);
        }

        protected static string Quote(string value)
        {
            return $@"""{value}""";
        }
    }

    public class ObjectToJsonConverter<T> : JsonConverter<T, String>
    {
        public ObjectToJsonConverter(JsonSerializerSettings settings, ISet<Type> stringTypes) : base(settings, stringTypes)
        {
            // there's nothing else to do
        }
        
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
            var result = JsonConvert.SerializeObject(context.Value, Settings);

            // String-types must not contain quotes after serialization.
            return
                IsStringType(typeof(T))
                    ? Unquote(result)
                    : result;
        }

        protected static string Unquote(string value)
        {
            return Regex.Replace(value, @"(^""|""$)", string.Empty);
        }
    }
}