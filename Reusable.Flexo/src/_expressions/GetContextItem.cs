using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Exceptionizer;

namespace Reusable.Flexo
{
    /// <summary>
    /// Gets an item from the context. Calls Invoke if it's an expression, otherwise it's wrapped in a Constant.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class GetContextItem : Expression
    {
        public GetContextItem() : base(nameof(GetContextItem)) { }

        [JsonRequired]
        public string Key { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            return
                context.TryGetValue(Key, out var value)
                    ? (value is IExpression expression ? expression.Invoke(context) : Constant.FromValue(Key, value))
                    : throw DynamicException.Create("KeyNotFound", $"Context does not contain an item with the key '{Key}'.");
        }
    }
}