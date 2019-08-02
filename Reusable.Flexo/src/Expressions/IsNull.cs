using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNull : ValueExtension<bool>
    {
        public IsNull(ILogger<IsNull> logger) : base(logger, nameof(IsNull)) { }
        
        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        protected override Constant<bool> InvokeCore()
        {
            var value = Left.Invoke().Value;
            return (Name, value is null);
        }
    }
}