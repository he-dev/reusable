using Newtonsoft.Json.Serialization;

namespace Reusable.Flexo
{
    public class IsEqual : PredicateExpression
    {
        public IsEqual(string name) : base(name ?? nameof(IsEqual), ExpressionContext.Empty) { }

        public IExpression Value { get; set; }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var other = ExtensionInputOrDefault(ref context, Constant.Null).Value<object>();
            return (Value.Invoke(context).Value<object>().Equals(other), context);
        }
    }

    // public class ExpressionConverter : JsonConverter<IExpression>
    // {
    //     public override void WriteJson(JsonWriter writer, IExpression value, JsonSerializer serializer)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public override IExpression ReadJson(JsonReader reader, Type objectType, IExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
    //     {
    //         if (!objectType.IsInterface)
    //         {
    //             var contract = serializer.ContractResolver.ResolveContract(objectType);
    //             var instance = contract.DefaultCreator();
    //             serializer.Populate(reader, instance);
    //             return (IExpression)instance; // serializer.Deserialize(reader, objectType);
    //         }
    //
    //         if (reader.Value is null && reader.TokenType == JsonToken.StartObject)
    //         {
    //             //reader.Read();
    //             //reader.Read();
    //             //var typeName = (string)reader.Value;
    //             //reader.Read();
    //
    //             //var jObject = JObject.Load(reader);
    //             var jToken = JObject.Load(reader);
    //             var typeName = (string)jToken.SelectToken("$.$type"); //jToken.SelectToken() jObject.va Type.GetType((string)reader.Value);
    //             objectType = Type.GetType(typeName);
    //             var result = jToken.ToObject(objectType, serializer);
    //             return (IExpression)result;
    //             existingValue = existingValue ?? (IExpression)serializer.ContractResolver.ResolveContract(objectType).DefaultCreator();
    //             serializer.Populate(jToken.CreateReader(), existingValue);
    //             return existingValue;
    //
    //             //var jObject = JObject.Load(reader);
    //             //return jObject.ToObject<IExpression>(serializer);
    //             //return jToken.ToObject<IExpression>(serializer);
    //             //return serializer.Deserialize<IExpression>(reader);
    //         }
    //         else
    //         {
    //             var jToken = JToken.Load(reader);
    //         }
    //         throw new NotImplementedException();
    //     }
    // }
}