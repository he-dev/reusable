using System;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : Expression<object>, IExtension<bool>
    {
        public IIf() : base(nameof(IIf)) { }

        [JsonRequired]
        public IExpression Predicate { get; set; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        protected override Constant<object> InvokeCore(IExpressionContext context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");

            if (context.TryPopExtensionInput(out bool input))
            {
                if (input)
                {
                    var trueResult = True?.Invoke(context);
                    return (Name, trueResult?.Value, trueResult?.Context);
                }
                else
                {
                    var falseResult = False?.Invoke(context);
                    return (Name, falseResult?.Value, falseResult?.Context);
                }
            }
            else
            {
                var result = Predicate.Invoke(context);

                if (result.Value<bool>())
                {
                    var trueResult = True?.Invoke(result.Context);
                    return (Name, trueResult?.Value, trueResult?.Context);
                }
                else
                {
                    var falseResult = False?.Invoke(result.Context);
                    return (Name, falseResult?.Value, falseResult?.Context);
                }
            }
        }
    }
}