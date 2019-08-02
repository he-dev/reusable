using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : ValueExpressionExtension<bool>
    {
        public IsNullOrEmpty(ILogger<IsNullOrEmpty> logger) : base(logger, nameof(IsNullOrEmpty)) { }
        
        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        protected override Constant<bool> InvokeCore()
        {
            var value = (string)Left.Invoke().Value;
            return (Name, string.IsNullOrEmpty(value));
        }
    }
}