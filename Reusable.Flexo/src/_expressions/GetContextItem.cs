using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    /// <summary>
    /// Gets an item from the context. Calls Invoke if it's an expression, otherwise it's wrapped in a Constant.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class GetContextItem : Expression<object>
    {
        public GetContextItem() : base(nameof(GetContextItem)) { }

        [JsonRequired]
        public string Key { get; set; }

        protected override Constant<object> InvokeCore(IExpressionContext context)
        {
            return
                context.TryGetValue(Key, out var value)
                    ? (value.Name, value.Value, context) // (value is IExpression expression ? expression : Constant.FromValue(Key, value), context)
                    : throw DynamicException.Create("KeyNotFound", $"Context does not contain an item with the key '{Key}'.");
        }
    }
}